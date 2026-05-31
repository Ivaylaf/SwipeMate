using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwipeMate.Api.Data;
using SwipeMate.Api.Dtos;
using SwipeMate.Api.Models;
using System.Security.Claims;

namespace SwipeMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public FriendsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new Exception("No user id claim");

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string? q)
    {
        var meId = CurrentUserId;
        var query = q?.Trim();

        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(Array.Empty<object>());
        }

        var friendIds = await _db.Friendships
            .Where(f => f.UserAId == meId || f.UserBId == meId)
            .Select(f => f.UserAId == meId ? f.UserBId : f.UserAId)
            .ToListAsync();

        var pendingIds = await _db.FriendshipRequests
            .Where(r => r.Status == "Pending" && (r.FromUserId == meId || r.ToUserId == meId))
            .Select(r => r.FromUserId == meId ? r.ToUserId : r.FromUserId)
            .ToListAsync();

        var blockedIds = friendIds
            .Concat(pendingIds)
            .Append(meId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var users = await _userManager.Users
            .Where(u => !blockedIds.Contains(u.Id))
            .Where(u => !u.IsBlocked)
            .Where(u =>
                (u.UserName != null && EF.Functions.Like(u.UserName, $"%{query}%")) ||
                (u.DisplayName != null && EF.Functions.Like(u.DisplayName, $"%{query}%")) ||
                (u.Email != null && EF.Functions.Like(u.Email, $"%{query}%")))
            .OrderBy(u => u.UserName)
            .Take(8)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.DisplayName,
                u.ProfileImageUrl
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("request")]
    public async Task<IActionResult> SendRequest(FriendRequestDto dto)
    {
        var meId = CurrentUserId;
        var toUser = await _userManager.FindByNameAsync(dto.ToUserName);

        if (toUser == null)
            return NotFound("User not found");

        if (toUser.IsBlocked)
            return BadRequest("This user is blocked and cannot receive friend requests.");

        if (toUser.Id == meId)
            return BadRequest("You cannot add yourself");

        var alreadyFriends = await _db.Friendships.AnyAsync(f =>
            (f.UserAId == meId && f.UserBId == toUser.Id) ||
            (f.UserAId == toUser.Id && f.UserBId == meId));

        if (alreadyFriends)
            return BadRequest("Already friends");

        var existingRequest = await _db.FriendshipRequests.AnyAsync(r =>
            r.Status == "Pending" &&
            ((r.FromUserId == meId && r.ToUserId == toUser.Id) ||
             (r.FromUserId == toUser.Id && r.ToUserId == meId)));

        if (existingRequest)
            return BadRequest("Request already exists");

        var req = new FriendshipRequest
        {
            FromUserId = meId,
            ToUserId = toUser.Id,
            Status = "Pending"
        };

        _db.FriendshipRequests.Add(req);
        await _db.SaveChangesAsync();

        return Ok(new { requestId = req.Id });
    }

    [HttpGet("requests")]
    public async Task<IActionResult> IncomingRequests()
    {
        var meId = CurrentUserId;

        var reqs = await _db.FriendshipRequests
            .Where(r => r.ToUserId == meId && r.Status == "Pending")
            .Join(_userManager.Users,
                r => r.FromUserId,
                u => u.Id,
                (r, u) => new
                {
                    r.Id,
                    r.FromUserId,
                    u.UserName,
                    u.DisplayName,
                    u.ProfileImageUrl,
                    r.CreatedAtUtc
                })
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync();

        return Ok(reqs);
    }

    [HttpPost("respond")]
    public async Task<IActionResult> Respond(RespondFriendRequestDto dto)
    {
        var meId = CurrentUserId;

        var req = await _db.FriendshipRequests.FirstOrDefaultAsync(r => r.Id == dto.RequestId);
        if (req == null) return NotFound("Request not found");

        if (req.ToUserId != meId)
            return Forbid("Not your request");

        if (req.Status != "Pending")
            return BadRequest("Request already handled");

        if (!dto.Accept)
        {
            req.Status = "Rejected";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Rejected" });
        }

        req.Status = "Accepted";

        var a = string.CompareOrdinal(req.FromUserId, req.ToUserId) < 0 ? req.FromUserId : req.ToUserId;
        var b = a == req.FromUserId ? req.ToUserId : req.FromUserId;

        _db.Friendships.Add(new Friendship { UserAId = a, UserBId = b });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Accepted" });
    }

    [HttpGet]
    public async Task<IActionResult> ListFriends()
    {
        var meId = CurrentUserId;

        var friendIds = await _db.Friendships
            .Where(f => f.UserAId == meId || f.UserBId == meId)
            .Select(f => f.UserAId == meId ? f.UserBId : f.UserAId)
            .ToListAsync();

        var friends = await _userManager.Users
            .Where(u => friendIds.Contains(u.Id))
            .Where(u => !u.IsBlocked)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.DisplayName,
                u.ProfileImageUrl
            })
            .ToListAsync();

        return Ok(friends);
    }
}


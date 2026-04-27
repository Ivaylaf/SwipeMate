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
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public ProfileController(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new Exception("No user id claim");

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = CurrentUserId;
        var user = await _userManager.Users.FirstAsync(x => x.Id == userId);
        var sessionsCount = await _db.MatchSessionParticipants.CountAsync(x => x.UserId == userId);
        var sessionIds = await _db.MatchSessionParticipants
            .Where(x => x.UserId == userId)
            .Select(x => x.SessionId)
            .ToListAsync();
        var matchesCount = await _db.SessionMatches.CountAsync(x => sessionIds.Contains(x.SessionId));
        var ratingsCount = await _db.SwipeVotes.CountAsync(x => x.UserId == userId);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.ProfileImageUrl,
            MatchesCount = matchesCount,
            SessionsCount = sessionsCount,
            RatingsCount = ratingsCount
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.DisplayName = TrimOrNull(dto.DisplayName);
        user.Bio = TrimOrNull(dto.Bio);
        user.ProfileImageUrl = TrimOrNull(dto.ProfileImageUrl);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.ProfileImageUrl
        });
    }

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

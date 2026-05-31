using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwipeMate.Api.Data;
using SwipeMate.Api.Dtos;
using SwipeMate.Api.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SwipeMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public sealed class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No user id claim");

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.UserName)
            .ToListAsync();

        var userIds = users.Select(x => x.Id).ToList();
        var friendships = await _db.Friendships
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserAId) || userIds.Contains(x.UserBId))
            .ToListAsync();

        var sessionCounts = await _db.MatchSessionParticipants
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Select(x => x.SessionId).Distinct().Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var matchCounts = await _db.MatchSessionParticipants
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .Join(_db.SessionMatches.AsNoTracking(), p => p.SessionId, m => m.SessionId, (p, m) => new { p.UserId, m.SessionId, m.ItemId })
            .Distinct()
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var result = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var friendsCount = friendships.Count(x => x.UserAId == user.Id || x.UserBId == user.Id);

            result.Add(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.DisplayName,
                user.Bio,
                user.ProfileImageUrl,
                user.IsBlocked,
                user.BlockedAtUtc,
                user.BlockedReason,
                FriendsCount = friendsCount,
                SessionsCount = sessionCounts.GetValueOrDefault(user.Id, 0),
                MatchesCount = matchCounts.GetValueOrDefault(user.Id, 0),
                Roles = roles.OrderBy(x => x).ToList()
            });
        }

        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserDetails(string id)
    {
        var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var friendIds = await _db.Friendships
            .AsNoTracking()
            .Where(x => x.UserAId == id || x.UserBId == id)
            .Select(x => x.UserAId == id ? x.UserBId : x.UserAId)
            .Distinct()
            .ToListAsync();

        var recentSessions = await _db.MatchSessionParticipants
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .Join(_db.MatchSessions.AsNoTracking(), p => p.SessionId, s => s.Id, (p, s) => new
            {
                s.Id,
                s.Category,
                s.Status,
                s.CreatedAtUtc
            })
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(10)
            .ToListAsync();

        var recentMatches = await _db.MatchSessionParticipants
            .AsNoTracking()
            .Where(x => x.UserId == id)
            .Join(_db.SessionMatches.AsNoTracking(), p => p.SessionId, m => m.SessionId, (p, m) => new { p.SessionId, m.ItemId, m.CreatedAtUtc })
            .Join(_db.SessionItems.AsNoTracking(), x => x.ItemId, i => i.Id, (x, i) => new
            {
                x.SessionId,
                i.Title,
                i.Category,
                x.CreatedAtUtc
            })
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(10)
            .ToListAsync();

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.Bio,
            user.ProfileImageUrl,
            user.IsBlocked,
            user.BlockedAtUtc,
            user.BlockedReason,
            Roles = roles.OrderBy(x => x).ToList(),
            FriendsCount = friendIds.Count,
            SessionsCount = recentSessions.Select(x => x.Id).Distinct().Count(),
            MatchesCount = recentMatches.Count,
            RecentSessions = recentSessions,
            RecentMatches = recentMatches
        });
    }

    [HttpPost("users/{id}/block")]
    public async Task<IActionResult> BlockUser(string id, BlockUserDto dto)
    {
        if (string.Equals(id, CurrentUserId, StringComparison.Ordinal))
        {
            return BadRequest("You cannot block your own admin account.");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return BadRequest("Admin users cannot be blocked from this screen.");
        }

        user.IsBlocked = true;
        user.BlockedAtUtc = DateTime.UtcNow;
        user.BlockedReason = string.IsNullOrWhiteSpace(dto.Reason)
            ? "Blocked by administrator"
            : dto.Reason.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        return Ok(new { user.Id, user.UserName, user.IsBlocked, user.BlockedAtUtc, user.BlockedReason });
    }

    [HttpPost("users/{id}/unblock")]
    public async Task<IActionResult> UnblockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound("User not found.");
        }

        user.IsBlocked = false;
        user.BlockedAtUtc = null;
        user.BlockedReason = null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(x => x.Description));
        }

        return Ok(new { user.Id, user.UserName, user.IsBlocked });
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog()
    {
        var items = await _db.CatalogItems
            .AsNoTracking()
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Title)
            .ToListAsync();

        return Ok(items.Select(x => new
        {
            x.Id,
            x.Category,
            x.ExternalId,
            x.Title,
            x.ImageUrl,
            x.IsActive,
            x.CreatedAtUtc,
            Summary = BuildCatalogSummary(x.Category, x.MetaJson)
        }));
    }

    [HttpGet("catalog/{id:guid}")]
    public async Task<IActionResult> GetCatalogDetails(Guid id)
    {
        var item = await _db.CatalogItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound("Catalog item not found.");
        }

        var meta = ParseMeta(item.MetaJson);
        return Ok(new
        {
            item.Id,
            item.Category,
            item.ExternalId,
            item.Title,
            item.ImageUrl,
            item.IsActive,
            item.CreatedAtUtc,
            Summary = BuildCatalogSummary(item.Category, item.MetaJson),
            Description = GetString(meta, "description"),
            SourceName = GetString(meta, "sourceName"),
            SourceUrl = GetString(meta, "sourceUrl"),
            MetaJson = item.MetaJson
        });
    }

    [HttpPost("catalog/{id:guid}/status")]
    public async Task<IActionResult> SetCatalogStatus(Guid id, UpdateCatalogItemStatusDto dto)
    {
        var item = await _db.CatalogItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return NotFound("Catalog item not found.");
        }

        item.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();

        return Ok(new { item.Id, item.Title, item.IsActive });
    }

    [HttpGet("backup")]
    public async Task<IActionResult> ExportBackup()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email,
                x.DisplayName,
                x.Bio,
                x.ProfileImageUrl,
                x.IsBlocked,
                x.BlockedAtUtc,
                x.BlockedReason
            })
            .ToListAsync();

        var backup = new
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Users = users,
            CatalogItems = await _db.CatalogItems.AsNoTracking().OrderBy(x => x.Category).ThenBy(x => x.Title).ToListAsync(),
            Friendships = await _db.Friendships.AsNoTracking().ToListAsync(),
            FriendshipRequests = await _db.FriendshipRequests.AsNoTracking().ToListAsync(),
            MatchSessions = await _db.MatchSessions.AsNoTracking().ToListAsync(),
            SessionParticipants = await _db.MatchSessionParticipants.AsNoTracking().ToListAsync(),
            SessionInvitations = await _db.SessionInvitations.AsNoTracking().ToListAsync(),
            SessionFilters = await _db.SessionFilters.AsNoTracking().ToListAsync(),
            MovieSessionFilters = await _db.MovieSessionFilters.AsNoTracking().ToListAsync(),
            RestaurantSessionFilters = await _db.RestaurantSessionFilters.AsNoTracking().ToListAsync(),
            RecipeSessionFilters = await _db.RecipeSessionFilters.AsNoTracking().ToListAsync(),
            BoardGameSessionFilters = await _db.BoardGameSessionFilters.AsNoTracking().ToListAsync(),
            SwipeVotes = await _db.SwipeVotes.AsNoTracking().ToListAsync(),
            SessionMatches = await _db.SessionMatches.AsNoTracking().ToListAsync()
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", $"swipemate-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
    }

    private static JsonElement ParseMeta(string? metaJson)
        => string.IsNullOrWhiteSpace(metaJson)
            ? default
            : JsonSerializer.Deserialize<JsonElement>(metaJson);

    private static string? GetString(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value)
            ? value.GetString()
            : null;

    private static double? GetDouble(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.TryGetDouble(out var number)
            ? number
            : null;

    private static int? GetInt(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.TryGetInt32(out var number)
            ? number
            : null;

    private static List<string> GetStrings(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToList();
    }

    private static string BuildCatalogSummary(string category, string? metaJson)
    {
        var meta = ParseMeta(metaJson);

        return category switch
        {
            "Movie" => $"{GetString(meta, "kind") ?? "title"} • {GetInt(meta, "year")?.ToString() ?? "-"} • рейтинг {GetDouble(meta, "rating")?.ToString("0.0") ?? "-"}",
            "Restaurant" => $"{GetString(meta, "city") ?? "-"}, {GetString(meta, "district") ?? "-"} • {GetString(meta, "cuisine") ?? "-"} • рейтинг {GetDouble(meta, "rating")?.ToString("0.0") ?? "-"}",
            "Recipe" => $"{GetString(meta, "cuisine") ?? "-"} • {GetString(meta, "foodType") ?? "-"} • сложност {GetInt(meta, "complexity")?.ToString() ?? "-"}",
            "BoardGame" => $"{GetString(meta, "gameType") ?? "-"} • {GetInt(meta, "playersMin")?.ToString() ?? "-"}-{GetInt(meta, "playersMax")?.ToString() ?? "-"} играчи • рейтинг {GetDouble(meta, "rating")?.ToString("0.0") ?? "-"}",
            _ => GetString(meta, "description") ?? "Без допълнителна информация"
        };
    }
}


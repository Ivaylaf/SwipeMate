using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwipeMate.Api.Data;
using SwipeMate.Api.Dtos;
using SwipeMate.Api.Models;
using System.Security.Claims;

namespace SwipeMate.Api.Controllers;

[ApiController]
[Route("api/sessions/{sessionId:guid}/filters")]
[Authorize]
public class SessionRestaurantFiltersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SessionRestaurantFiltersController(AppDbContext db)
    {
        _db = db;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new Exception("No user id claim");

    // PUT: /api/sessions/{sessionId}/filters/restaurants
    [HttpPut("restaurants")]
    public async Task<IActionResult> UpsertMyRestaurantFilters(Guid sessionId, UpdateRestaurantFiltersDto dto)
    {
        var exists = await _db.MatchSessions.AnyAsync(s => s.Id == sessionId);
        if (!exists) return NotFound(new { message = "Session not found" });

        var userId = CurrentUserId;

        var entity = await _db.RestaurantSessionFilters
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (entity == null)
        {
            entity = new RestaurantSessionFilter
            {
                SessionId = sessionId,
                UserId = userId
            };
            _db.RestaurantSessionFilters.Add(entity);
        }

        entity.City = Normalize(dto.City);
        entity.District = Normalize(dto.District);
        entity.Cuisine = Normalize(dto.Cuisine);
        entity.MinRating = dto.MinRating;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Saved" });
    }

    // GET: /api/sessions/{sessionId}/filters/restaurants/me
    [HttpGet("restaurants/me")]
    public async Task<IActionResult> GetMyRestaurantFilters(Guid sessionId)
    {
        var userId = CurrentUserId;

        var entity = await _db.RestaurantSessionFilters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (entity == null) return Ok(new RestaurantFiltersDto());

        return Ok(ToDto(entity));
    }

    // GET: /api/sessions/{sessionId}/filters/restaurants/merged
    // Обединение:
    // - City: ако има поне една стойност -> взимаме най-често срещаната (mode), иначе null
    // - District: mode
    // - Cuisine: ако различни -> null (за MVP, иначе ще стане усложнение)
    // - MinRating: MIN (за да не изключим изборите на някого)
    [HttpGet("restaurants/merged")]
    public async Task<IActionResult> GetMergedRestaurantFilters(Guid sessionId)
    {
        var filters = await _db.RestaurantSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (filters.Count == 0) return Ok(new RestaurantFiltersDto());

        string? city = Mode(filters.Select(f => f.City));
        string? district = Mode(filters.Select(f => f.District));

        // Cuisine: ако всички непразни са еднакви -> ползваме я, иначе null
        var cuisines = filters
            .Select(f => f.Cuisine)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        string? cuisine = cuisines.Count == 1 ? cuisines[0] : null;

        double? minRating = filters.Where(f => f.MinRating.HasValue).Select(f => f.MinRating!.Value).DefaultIfEmpty().Min();
        if (!filters.Any(f => f.MinRating.HasValue)) minRating = null;

        return Ok(new RestaurantFiltersDto
        {
            City = city,
            District = district,
            Cuisine = cuisine,
            MinRating = minRating
        });
    }

    private static string? Normalize(string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static RestaurantFiltersDto ToDto(RestaurantSessionFilter f) => new()
    {
        City = f.City,
        District = f.District,
        Cuisine = f.Cuisine,
        MinRating = f.MinRating
    };

    private static string? Mode(IEnumerable<string?> values)
    {
        var groups = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .GroupBy(v => v, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ToList();

        return groups.Count == 0 ? null : groups[0].Key;
    }
}

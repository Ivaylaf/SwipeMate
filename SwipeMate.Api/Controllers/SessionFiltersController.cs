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
public class SessionFiltersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SessionFiltersController(AppDbContext db)
    {
        _db = db;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new Exception("No user id claim");

    [HttpPut("movies")]
    public async Task<IActionResult> UpsertMyMovieFilters(Guid sessionId, UpdateMovieFiltersDto dto)
    {
        var exists = await _db.MatchSessions.AnyAsync(s => s.Id == sessionId);
        if (!exists) return NotFound(new { message = "Session not found" });

        var userId = CurrentUserId;

        var entity = await _db.MovieSessionFilters
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (entity == null)
        {
            entity = new MovieSessionFilter
            {
                SessionId = sessionId,
                UserId = userId
            };
            _db.MovieSessionFilters.Add(entity);
        }

        entity.MinRating = dto.MinRating;
        entity.YearFrom = dto.YearFrom;
        entity.YearTo = dto.YearTo;

        entity.GenresCsv = dto.Genres is { Count: > 0 }
            ? string.Join(",", dto.Genres.Select(g => g.Trim()).Where(g => g.Length > 0))
            : null;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Saved" });
    }

    [HttpGet("movies/me")]
    public async Task<IActionResult> GetMyMovieFilters(Guid sessionId)
    {
        var userId = CurrentUserId;

        var entity = await _db.MovieSessionFilters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (entity == null)
        {
            return Ok(new MovieFiltersDto());
        }

        return Ok(ToDto(entity));
    }

    [HttpGet("movies/merged")]
    public async Task<IActionResult> GetMergedMovieFilters(Guid sessionId)
    {
        var filters = await _db.MovieSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (filters.Count == 0)
        {
            return Ok(new MovieFiltersDto());
        }

        var allGenres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in filters)
        {
            if (!string.IsNullOrWhiteSpace(f.GenresCsv))
            {
                foreach (var g in f.GenresCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    allGenres.Add(g.Trim());
            }
        }

        double? minRating = filters.Where(f => f.MinRating.HasValue).Select(f => f.MinRating!.Value).DefaultIfEmpty().Min();
        if (!filters.Any(f => f.MinRating.HasValue)) minRating = null;

        int? yearFrom = filters.Where(f => f.YearFrom.HasValue).Select(f => f.YearFrom!.Value).DefaultIfEmpty().Min();
        if (!filters.Any(f => f.YearFrom.HasValue)) yearFrom = null;

        int? yearTo = filters.Where(f => f.YearTo.HasValue).Select(f => f.YearTo!.Value).DefaultIfEmpty().Max();
        if (!filters.Any(f => f.YearTo.HasValue)) yearTo = null;

        return Ok(new MovieFiltersDto
        {
            Genres = allGenres.OrderBy(x => x).ToList(),
            MinRating = minRating,
            YearFrom = yearFrom,
            YearTo = yearTo
        });
    }

    private static MovieFiltersDto ToDto(MovieSessionFilter f)
    {
        var genres = new List<string>();
        if (!string.IsNullOrWhiteSpace(f.GenresCsv))
        {
            genres = f.GenresCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return new MovieFiltersDto
        {
            Genres = genres,
            MinRating = f.MinRating,
            YearFrom = f.YearFrom,
            YearTo = f.YearTo
        };
    }
}



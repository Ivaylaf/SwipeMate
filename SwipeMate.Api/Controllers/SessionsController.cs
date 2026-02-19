using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwipeMate.Api.Data;
using SwipeMate.Api.Dtos;
using SwipeMate.Api.Models;
using System;
using System.Security.Claims;

namespace SwipeMate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SessionsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new Exception("No user id claim");

    // POST /api/sessions
    [HttpPost]
    public async Task<IActionResult> Create(CreateSessionDto dto)
    {
        var meId = CurrentUserId;

        var session = new MatchSession
        {
            Category = dto.Category,
            CreatedByUserId = meId,
            Status = "Active"
        };

        _db.MatchSessions.Add(session);

        // creator is participant
        _db.MatchSessionParticipants.Add(new MatchSessionParticipant
        {
            SessionId = session.Id,
            UserId = meId
        });

        // add friends by username
        foreach (var username in dto.FriendUserNames.Distinct())
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest($"User not found: {username}");

            _db.MatchSessionParticipants.Add(new MatchSessionParticipant
            {
                SessionId = session.Id,
                UserId = user.Id
            });
        }

        // seed test items for this session (MVP)
        SeedTestItems(session.Id, dto.Category);

        await _db.SaveChangesAsync();

        return Ok(new { sessionId = session.Id });
    }

    private void SeedTestItems(Guid sessionId, string category)
    {
        // ако вече има items, не seed-ваме пак
        var hasAny = _db.SessionItems.Any(i => i.SessionId == sessionId);
        if (hasAny) return;

        var items = new List<SessionItem>();

        // 10 тестови предложения
        for (int i = 1; i <= 10; i++)
        {
            items.Add(new SessionItem
            {
                SessionId = sessionId,
                Category = category,
                ExternalId = $"{category.ToLower()}_{i}",
                Title = $"{category} option #{i}"
            });
        }

        _db.SessionItems.AddRange(items);
    }

    // GET /api/sessions/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var session = await _db.MatchSessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();

        var participants = await _db.MatchSessionParticipants
            .Where(p => p.SessionId == id)
            .Select(p => p.UserId)
            .ToListAsync();

        return Ok(new { session.Id, session.Category, session.Status, participants });
    }

    // GET /api/sessions/{id}/next  -> връща първия item, за който текущият user още няма vote
    [HttpGet("{id:guid}/next")]
    public async Task<IActionResult> Next(Guid id)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var next = await _db.SessionItems
            .Where(i => i.SessionId == id)
    // пропусни item-и, които вече са match-нати
            .Where(i => !_db.SessionMatches.Any(m => m.SessionId == id && m.ItemId == i.Id))
    // пропусни item-и, за които този user вече е гласувал
            .Where(i => !_db.SwipeVotes.Any(v => v.SessionId == id && v.ItemId == i.Id && v.UserId == meId))
            .OrderBy(i => i.Id)
            .Select(i => new { i.Id, i.Title, i.Category, i.ImageUrl })
            .FirstOrDefaultAsync();


        if (next == null) return Ok(null); // няма повече

        return Ok(next);
    }

    // POST /api/sessions/{id}/swipe
    [HttpPost("{id:guid}/swipe")]
    public async Task<IActionResult> Swipe(Guid id, SwipeDto dto)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var item = await _db.SessionItems.FirstOrDefaultAsync(i => i.Id == dto.ItemId && i.SessionId == id);
        if (item == null) return NotFound("Item not found in this session");

        var alreadyVoted = await _db.SwipeVotes.AnyAsync(v =>
            v.SessionId == id && v.ItemId == dto.ItemId && v.UserId == meId);

        if (alreadyVoted) return BadRequest("Already voted");

        _db.SwipeVotes.Add(new SwipeVote
        {
            SessionId = id,
            ItemId = dto.ItemId,
            UserId = meId,
            IsYes = dto.IsYes
        });

        await _db.SaveChangesAsync();

        // check match: всички участници Yes
        if (dto.IsYes)
        {
            var participantIds = await _db.MatchSessionParticipants
                .Where(p => p.SessionId == id)
                .Select(p => p.UserId)
                .ToListAsync();

            var yesVoters = await _db.SwipeVotes
                .Where(v => v.SessionId == id && v.ItemId == dto.ItemId && v.IsYes)
                .Select(v => v.UserId)
                .ToListAsync();

            var allYes = participantIds.All(pid => yesVoters.Contains(pid));

            if (allYes)
            {
                var alreadyMatched = await _db.SessionMatches.AnyAsync(m => m.SessionId == id && m.ItemId == dto.ItemId);
                if (!alreadyMatched)
                {
                    _db.SessionMatches.Add(new SessionMatch { SessionId = id, ItemId = dto.ItemId });
                    await _db.SaveChangesAsync();
                }
            }
        }

        return Ok(new { ok = true });
    }

    // GET /api/sessions/{id}/matches
    [HttpGet("{id:guid}/matches")]
    public async Task<IActionResult> Matches(Guid id)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var matches = await _db.SessionMatches
            .Where(m => m.SessionId == id)
            .Join(_db.SessionItems, m => m.ItemId, i => i.Id,
                (m, i) => new { i.Id, i.Title, m.CreatedAtUtc })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(matches);
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CloseSessionDto dto)
    {
        var meId = CurrentUserId;

        var session = await _db.MatchSessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();

        if (session.CreatedByUserId != meId) return Forbid("Only creator can close session");

        session.Status = dto.Close ? "Closed" : "Active";
        await _db.SaveChangesAsync();

        return Ok(new { session.Id, session.Status });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> MySessions()
    {
        var meId = CurrentUserId;

        var sessions = await _db.MatchSessionParticipants
            .Where(p => p.UserId == meId)
            .Join(_db.MatchSessions, p => p.SessionId, s => s.Id,
                (p, s) => new { s.Id, s.Category, s.Status, s.CreatedAtUtc, s.CreatedByUserId })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("mine/matches")]
    public async Task<IActionResult> MyMatches()
    {
        var meId = CurrentUserId;

        var sessionIds = await _db.MatchSessionParticipants
            .Where(p => p.UserId == meId)
            .Select(p => p.SessionId)
            .ToListAsync();

        var matches = await _db.SessionMatches
            .Where(m => sessionIds.Contains(m.SessionId))
            .Join(_db.SessionItems, m => m.ItemId, i => i.Id,
                (m, i) => new { m.SessionId, i.Id, i.Title, i.Category, m.CreatedAtUtc })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(matches);
    }

    [HttpPut("{id:guid}/filters")]
    public async Task<IActionResult> UpdateMyFilters(Guid id, [FromBody] UpdateSessionFilterDto dto)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var existing = await _db.SessionFilters
            .FirstOrDefaultAsync(f => f.SessionId == id && f.UserId == meId);

        if (existing == null)
        {
            _db.SessionFilters.Add(new SessionFilter
            {
                SessionId = id,
                UserId = meId,
                FilterJson = dto.FilterJson,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.FilterJson = dto.FilterJson;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { ok = true });
    }

    [HttpGet("{id:guid}/filters/merged")]
    public async Task<IActionResult> GetMergedFilters(Guid id)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var filters = await _db.SessionFilters
            .Where(f => f.SessionId == id)
            .Select(f => f.FilterJson)
            .ToListAsync();

        // MVP: връщаме списък от JSON-и и UI решава как да ги “обедини”
        // (по-късно можем да направим истинско merge в бекенда)
        return Ok(new { filters });
    }


[HttpPut("{sessionId:guid}/filters/recipes")]
public async Task<IActionResult> PutMyRecipeFilters(Guid sessionId, RecipeFiltersDto dto)
{
    var userId = CurrentUserId;

    var existing = await _db.RecipeSessionFilters
        .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

    var ingredientsCsv = (dto.Ingredients ?? new List<string>())
        .Select(x => x.Trim())
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Distinct(StringComparer.OrdinalIgnoreCase);

    if (existing == null)
    {
        existing = new RecipeSessionFilter
        {
            SessionId = sessionId,
            UserId = userId
        };
        _db.RecipeSessionFilters.Add(existing);
    }

    existing.Complexity = dto.Complexity;
    existing.Cuisine = dto.Cuisine;
    existing.FoodType = dto.FoodType;
    existing.BudgetLevel = dto.BudgetLevel;
    existing.MinRating = dto.MinRating;
    existing.IngredientsCsv = string.Join(",", ingredientsCsv);

    await _db.SaveChangesAsync();
    return Ok();
}

    [HttpGet("{sessionId:guid}/filters/recipes/me")]
    public async Task<ActionResult<RecipeFiltersDto>> GetMyRecipeFilters(Guid sessionId)
    {
        var userId = CurrentUserId;

        var f = await _db.RecipeSessionFilters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (f == null)
            return Ok(new RecipeFiltersDto());

        return Ok(new RecipeFiltersDto
        {
            Complexity = f.Complexity,
            Cuisine = f.Cuisine,
            FoodType = f.FoodType,
            BudgetLevel = f.BudgetLevel,
            MinRating = f.MinRating,
            Ingredients = string.IsNullOrWhiteSpace(f.IngredientsCsv)
                ? new List<string>()
                : f.IngredientsCsv.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList()
        });
    }

    [HttpGet("{sessionId:guid}/filters/recipes/merged")]
    public async Task<ActionResult<RecipeFiltersDto>> GetMergedRecipeFilters(Guid sessionId)
    {
        var rows = await _db.RecipeSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (rows.Count == 0)
            return Ok(new RecipeFiltersDto());

        int? complexity = rows.Where(r => r.Complexity.HasValue).Select(r => r.Complexity!.Value).DefaultIfEmpty().Min();
        double? minRating = rows.Where(r => r.MinRating.HasValue).Select(r => r.MinRating!.Value).DefaultIfEmpty().Min();
        int? budget = rows.Where(r => r.BudgetLevel.HasValue).Select(r => r.BudgetLevel!.Value).DefaultIfEmpty().Max();

        string? cuisine = rows.Select(r => r.Cuisine).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count() == 1
            ? rows.Select(r => r.Cuisine).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            : null;

        string? foodType = rows.Select(r => r.FoodType).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Count() == 1
            ? rows.Select(r => r.FoodType).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            : null;

        var ingredients = rows
            .SelectMany(r => string.IsNullOrWhiteSpace(r.IngredientsCsv) ? Array.Empty<string>() : r.IngredientsCsv.Split(','))
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(new RecipeFiltersDto
        {
            Complexity = complexity == 0 ? null : complexity,
            MinRating = minRating == 0 ? null : minRating,
            BudgetLevel = budget == 0 ? null : budget,
            Cuisine = cuisine,
            FoodType = foodType,
            Ingredients = ingredients
        });
    }


    [HttpPut("{sessionId:guid}/filters/games")]
    public async Task<IActionResult> PutMyBoardGameFilters(Guid sessionId, BoardGameFiltersDto dto)
    {
        var userId = CurrentUserId;

        var existing = await _db.BoardGameSessionFilters
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (existing == null)
        {
            existing = new BoardGameSessionFilter
            {
                SessionId = sessionId,
                UserId = userId
            };
            _db.BoardGameSessionFilters.Add(existing);
        }

        existing.GameType = string.IsNullOrWhiteSpace(dto.GameType) ? null : dto.GameType.Trim();
        existing.DurationMin = dto.DurationMin;
        existing.DurationMax = dto.DurationMax;
        existing.PlayersMin = dto.PlayersMin;
        existing.PlayersMax = dto.PlayersMax;
        existing.MinRating = dto.MinRating;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{sessionId:guid}/filters/games/me")]
    public async Task<ActionResult<BoardGameFiltersDto>> GetMyBoardGameFilters(Guid sessionId)
    {
        var userId = CurrentUserId;

        var f = await _db.BoardGameSessionFilters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (f == null) return Ok(new BoardGameFiltersDto());

        return Ok(new BoardGameFiltersDto
        {
            GameType = f.GameType,
            DurationMin = f.DurationMin,
            DurationMax = f.DurationMax,
            PlayersMin = f.PlayersMin,
            PlayersMax = f.PlayersMax,
            MinRating = f.MinRating
        });
    }


    [HttpGet("{sessionId:guid}/filters/games/merged")]
    public async Task<ActionResult<BoardGameFiltersDto>> GetMergedBoardGameFilters(Guid sessionId)
    {
        var rows = await _db.BoardGameSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (rows.Count == 0) return Ok(new BoardGameFiltersDto());

        var gameTypes = rows
            .Select(r => r.GameType)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        string? gameType = gameTypes.Count == 1 ? gameTypes[0] : null;

        int? durationMin = rows.Where(r => r.DurationMin.HasValue).Select(r => r.DurationMin!.Value).DefaultIfEmpty().Min();
        if (!rows.Any(r => r.DurationMin.HasValue)) durationMin = null;

        int? durationMax = rows.Where(r => r.DurationMax.HasValue).Select(r => r.DurationMax!.Value).DefaultIfEmpty().Max();
        if (!rows.Any(r => r.DurationMax.HasValue)) durationMax = null;

        int? playersMin = rows.Where(r => r.PlayersMin.HasValue).Select(r => r.PlayersMin!.Value).DefaultIfEmpty().Min();
        if (!rows.Any(r => r.PlayersMin.HasValue)) playersMin = null;

        int? playersMax = rows.Where(r => r.PlayersMax.HasValue).Select(r => r.PlayersMax!.Value).DefaultIfEmpty().Max();
        if (!rows.Any(r => r.PlayersMax.HasValue)) playersMax = null;

        double? minRating = rows.Where(r => r.MinRating.HasValue).Select(r => r.MinRating!.Value).DefaultIfEmpty().Min();
        if (!rows.Any(r => r.MinRating.HasValue)) minRating = null;

        return Ok(new BoardGameFiltersDto
        {
            GameType = gameType,
            DurationMin = durationMin,
            DurationMax = durationMax,
            PlayersMin = playersMin,
            PlayersMax = playersMax,
            MinRating = minRating
        });
    }







}


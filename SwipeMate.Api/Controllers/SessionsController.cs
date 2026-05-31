using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwipeMate.Api.Data;
using SwipeMate.Api.Dtos;
using SwipeMate.Api.Models;
using System;
using System.Security.Claims;
using System.Text.Json;

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
        var category = DemoCatalog.NormalizeCategory(dto.Category);
        var friendUserNames = dto.FriendUserNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (friendUserNames.Count == 0)
        {
            return BadRequest("Select at least one friend to create a group session.");
        }

        var invitedUsers = new List<ApplicationUser>();

        foreach (var username in friendUserNames)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest($"User not found: {username}");
            }

            if (user.Id == meId)
            {
                continue;
            }

            var areFriends = await _db.Friendships.AnyAsync(f =>
                (f.UserAId == meId && f.UserBId == user.Id) ||
                (f.UserAId == user.Id && f.UserBId == meId));

            if (!areFriends)
            {
                return BadRequest($"{username} is not in your friends list.");
            }

            invitedUsers.Add(user);
        }

        if (invitedUsers.Count == 0)
        {
            return BadRequest("Select at least one valid friend to create a group session.");
        }

        var session = new MatchSession
        {
            Category = category,
            CreatedByUserId = meId,
            Status = "Pending"
        };

        _db.MatchSessions.Add(session);
        _db.MatchSessionParticipants.Add(new MatchSessionParticipant
        {
            SessionId = session.Id,
            UserId = meId
        });

        foreach (var invitedUser in invitedUsers)
        {
            _db.SessionInvitations.Add(new SessionInvitation
            {
                SessionId = session.Id,
                InvitedUserId = invitedUser.Id,
                InvitedByUserId = meId,
                Status = "Pending"
            });
        }

        await _db.SaveChangesAsync();
        await SeedSessionItemsFromCatalogAsync(session.Id, category);
        await RefreshSessionStatusAsync(session.Id);

        return Ok(new { sessionId = session.Id, category, status = session.Status, invitedCount = invitedUsers.Count });
    }

    private async Task SeedSessionItemsFromCatalogAsync(Guid sessionId, string category)
    {
        var hasAny = await _db.SessionItems.AnyAsync(i => i.SessionId == sessionId);
        if (hasAny) return;

        var catalogItems = await _db.CatalogItems
            .AsNoTracking()
            .Where(x => x.Category == category && x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync();

        var sessionItems = catalogItems.Count > 0
            ? catalogItems.Select(item => new SessionItem
            {
                SessionId = sessionId,
                Category = item.Category,
                ExternalId = item.ExternalId,
                Title = item.Title,
                ImageUrl = item.ImageUrl,
                MetaJson = item.MetaJson
            }).ToList()
            : DemoCatalog.CreateSessionItems(sessionId, category);

        _db.SessionItems.AddRange(sessionItems);
        await _db.SaveChangesAsync();
    }
    [HttpGet("invitations")]
    public async Task<IActionResult> MyInvitations()
    {
        var meId = CurrentUserId;

        var terminalStatuses = new[] { "Finished", "Closed", "Declined", "Expired", "Cancelled" };

        var invitations = await _db.SessionInvitations
            .Where(x => x.InvitedUserId == meId && x.Status == "Pending")
            .Join(_db.MatchSessions, x => x.SessionId, s => s.Id, (x, s) => new { Invitation = x, Session = s })
            .Where(x => !terminalStatuses.Contains(x.Session.Status))
            .Join(_userManager.Users,
                x => x.Invitation.InvitedByUserId,
                u => u.Id,
                (x, u) => new
                {
                    x.Invitation.Id,
                    x.Invitation.SessionId,
                    x.Session.Category,
                    x.Session.Status,
                    x.Invitation.CreatedAtUtc,
                    InvitedByUserName = u.UserName,
                    InvitedByDisplayName = u.DisplayName
                })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        return Ok(invitations);
    }

    [HttpPost("invitations/respond")]
    public async Task<IActionResult> RespondToInvitation(RespondSessionInvitationDto dto)
    {
        var meId = CurrentUserId;

        var invitation = await _db.SessionInvitations.FirstOrDefaultAsync(x => x.Id == dto.InvitationId && x.InvitedUserId == meId);
        if (invitation == null)
        {
            return NotFound("Invitation not found.");
        }

        if (!string.Equals(invitation.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Invitation already answered.");
        }

        invitation.Status = dto.Accept ? "Accepted" : "Declined";
        invitation.RespondedAtUtc = DateTime.UtcNow;

        if (dto.Accept)
        {
            var alreadyParticipant = await _db.MatchSessionParticipants.AnyAsync(x => x.SessionId == invitation.SessionId && x.UserId == meId);
            if (!alreadyParticipant)
            {
                _db.MatchSessionParticipants.Add(new MatchSessionParticipant
                {
                    SessionId = invitation.SessionId,
                    UserId = meId
                });
            }
        }

        await _db.SaveChangesAsync();
        await RefreshSessionStatusAsync(invitation.SessionId);

        return Ok(new { message = dto.Accept ? "Invitation accepted" : "Invitation declined" });
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
            .Join(_userManager.Users,
                p => p.UserId,
                u => u.Id,
                (p, u) => new
                {
                    u.Id,
                    u.UserName,
                    u.DisplayName
                })
            .ToListAsync();

        return Ok(new { session.Id, session.Category, session.Status, participants });
    }

    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var session = await _db.MatchSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();

        var participants = await _db.MatchSessionParticipants
            .Where(p => p.SessionId == id)
            .Join(_userManager.Users, p => p.UserId, u => u.Id, (p, u) => new
            {
                u.Id,
                u.UserName,
                u.DisplayName
            })
            .OrderBy(x => x.UserName)
            .ToListAsync();

        var swipeCount = await _db.SwipeVotes.CountAsync(v => v.SessionId == id);
        var matchCount = await _db.SessionMatches.CountAsync(m => m.SessionId == id);
        var pendingInvitationCount = await _db.SessionInvitations.CountAsync(i => i.SessionId == id && i.Status == "Pending");
        var filtersSummary = await BuildFilterSummaryAsync(id, session.Category);

        return Ok(new
        {
            session.Id,
            session.Category,
            session.Status,
            session.CreatedAtUtc,
            session.CreatedByUserId,
            SwipeCount = swipeCount,
            MatchCount = matchCount,
            PendingInvitationCount = pendingInvitationCount,
            Participants = participants,
            FiltersSummary = filtersSummary
        });
    }


    [HttpGet("{id:guid}/available-count")]
    public async Task<IActionResult> AvailableCount(Guid id)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var session = await _db.MatchSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();

        var candidates = await _db.SessionItems
            .Where(i => i.SessionId == id)
            .Where(i => !_db.SessionMatches.Any(m => m.SessionId == id && m.ItemId == i.Id))
            .OrderBy(i => i.Id)
            .ToListAsync();

        var filtered = await ApplyFiltersAsync(id, session.Category, candidates);
        return Ok(new { Count = filtered.Count });
    }
    // GET /api/sessions/{id}/next  -> vrushta purviya item, za koyto tekushtiyat user oshte nyama vote
    [HttpGet("{id:guid}/next")]
    public async Task<IActionResult> Next(Guid id)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var session = await _db.MatchSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();
        if (!string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("This session is not active yet. Wait for invitations to be accepted or refresh the session state.");
        }

        var candidates = await _db.SessionItems
            .Where(i => i.SessionId == id)
            .Where(i => !_db.SessionMatches.Any(m => m.SessionId == id && m.ItemId == i.Id))
            .Where(i => !_db.SwipeVotes.Any(v => v.SessionId == id && v.ItemId == i.Id && v.UserId == meId))
            .OrderBy(i => i.Id)
            .ToListAsync();

        var filtered = await ApplyFiltersAsync(id, session.Category, candidates);
        var filteredIds = filtered.Select(i => i.Id).ToHashSet();
        var votedItemIds = await _db.SwipeVotes
            .Where(v => v.SessionId == id)
            .Select(v => v.ItemId)
            .Distinct()
            .ToListAsync();
        var votedItemIdSet = votedItemIds.ToHashSet();

        var next = filtered
            .Concat(candidates.Where(i => votedItemIdSet.Contains(i.Id) && !filteredIds.Contains(i.Id)))
            .OrderBy(i => i.Id)
            .FirstOrDefault();

        if (next == null)
        {
            await RefreshSessionCompletionAsync(id);
            return Ok(null);
        }

        return Ok(ToItemResponse(next));
    }

    // POST /api/sessions/{id}/swipe
    [HttpPost("{id:guid}/swipe")]
    public async Task<IActionResult> Swipe(Guid id, SwipeDto dto)
    {
        var meId = CurrentUserId;

        var isParticipant = await _db.MatchSessionParticipants.AnyAsync(p => p.SessionId == id && p.UserId == meId);
        if (!isParticipant) return Forbid();

        var session = await _db.MatchSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();
        if (!string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("This session is not active yet.");
        }

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

        var participantIds = await _db.MatchSessionParticipants
            .Where(p => p.SessionId == id)
            .Select(p => p.UserId)
            .ToListAsync();

        var yesVoters = await _db.SwipeVotes
            .Where(v => v.SessionId == id && v.ItemId == dto.ItemId && v.IsYes)
            .Select(v => v.UserId)
            .ToListAsync();

        var matchedUserNames = yesVoters.Count >= 2
            ? await _userManager.Users
                .Where(u => yesVoters.Contains(u.Id))
                .OrderBy(u => u.UserName)
                .Select(u => u.UserName ?? u.DisplayName ?? "User")
                .ToListAsync()
            : new List<string>();

        var fullGroupMatch = dto.IsYes && participantIds.Count > 0 && participantIds.All(pid => yesVoters.Contains(pid));

        if (fullGroupMatch)
        {
            var alreadyMatched = await _db.SessionMatches.AnyAsync(m => m.SessionId == id && m.ItemId == dto.ItemId);
            if (!alreadyMatched)
            {
                _db.SessionMatches.Add(new SessionMatch { SessionId = id, ItemId = dto.ItemId });
                await _db.SaveChangesAsync();
            }
        }

        await RefreshSessionCompletionAsync(id);
        var updatedSession = await _db.MatchSessions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

        return Ok(new
        {
            ok = true,
            matchFound = matchedUserNames.Count >= 2,
            fullGroupMatch,
            matchedUsers = matchedUserNames,
            sessionStatus = updatedSession?.Status ?? session.Status
        });
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
                (m, i) => new
                {
                    m.SessionId,
                    i.Id,
                    i.Title,
                    i.Category,
                    i.ImageUrl,
                    i.MetaJson,
                    m.CreatedAtUtc
                })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        var matchedUsers = await _db.MatchSessionParticipants
            .Where(p => p.SessionId == id)
            .Join(_userManager.Users,
                p => p.UserId,
                u => u.Id,
                (p, u) => u.UserName ?? u.DisplayName ?? "User")
            .OrderBy(x => x)
            .ToListAsync();

        return Ok(matches.Select(x => ToItemResponse(new
        {
            x.SessionId,
            x.Id,
            x.Title,
            x.Category,
            x.ImageUrl,
            x.MetaJson,
            x.CreatedAtUtc,
            MatchedUsers = matchedUsers
        })));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CloseSessionDto dto)
    {
        var meId = CurrentUserId;

        var session = await _db.MatchSessions.FirstOrDefaultAsync(s => s.Id == id);
        if (session == null) return NotFound();

        if (session.CreatedByUserId != meId) return Forbid("Only creator can close session");

        session.Status = dto.Close ? "Closed" : "Active";

        if (dto.Close)
        {
            var pendingInvitations = await _db.SessionInvitations
                .Where(x => x.SessionId == id && x.Status == "Pending")
                .ToListAsync();

            foreach (var invitation in pendingInvitations)
            {
                invitation.Status = "Cancelled";
                invitation.RespondedAtUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { session.Id, session.Status });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> MySessions()
    {
        var meId = CurrentUserId;

        var sessionIds = await _db.MatchSessionParticipants
            .Where(p => p.UserId == meId)
            .Select(p => p.SessionId)
            .Distinct()
            .ToListAsync();

        foreach (var sessionId in sessionIds)
        {
            await ExpireStaleSessionAsync(sessionId);
            await RefreshSessionStatusAsync(sessionId);
            await RefreshSessionCompletionAsync(sessionId);
        }

        var sessions = await _db.MatchSessions
            .Where(s => sessionIds.Contains(s.Id))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(s => new { s.Id, s.Category, s.Status, s.CreatedAtUtc, s.CreatedByUserId })
            .ToListAsync();

        var participantCounts = await _db.MatchSessionParticipants
            .Where(p => sessionIds.Contains(p.SessionId))
            .GroupBy(p => p.SessionId)
            .Select(g => new { SessionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SessionId, x => x.Count);

        return Ok(sessions.Select(s => new
        {
            s.Id,
            s.Category,
            s.Status,
            s.CreatedAtUtc,
            s.CreatedByUserId,
            IsOwner = s.CreatedByUserId == meId,
            ParticipantCount = participantCounts.GetValueOrDefault(s.Id, 0)
        }));
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
                (m, i) => new
                {
                    m.SessionId,
                    i.Id,
                    i.Title,
                    i.Category,
                    i.ImageUrl,
                    i.MetaJson,
                    m.CreatedAtUtc
                })
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        var matchedUsersLookup = await _db.MatchSessionParticipants
            .Where(p => sessionIds.Contains(p.SessionId))
            .Join(_userManager.Users,
                p => p.UserId,
                u => u.Id,
                (p, u) => new
                {
                    p.SessionId,
                    Name = u.UserName ?? u.DisplayName ?? "User"
                })
            .OrderBy(x => x.Name)
            .GroupBy(x => x.SessionId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        return Ok(matches.Select(x => ToItemResponse(new
        {
            x.SessionId,
            x.Id,
            x.Title,
            x.Category,
            x.ImageUrl,
            x.MetaJson,
            x.CreatedAtUtc,
            MatchedUsers = matchedUsersLookup.GetValueOrDefault(x.SessionId, [])
        })));
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

        // MVP: ������� ������ �� JSON-� � UI ������ ��� �� �� ��������
        // (��-����� ����� �� �������� �������� merge � �������)
        return Ok(new { filters });
    }

    [HttpPut("{sessionId:guid}/filters/restaurants")]
    public async Task<IActionResult> PutMyRestaurantFilters(Guid sessionId, RestaurantFiltersDto dto)
    {
        var userId = CurrentUserId;

        var existing = await _db.RestaurantSessionFilters
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (existing == null)
        {
            existing = new RestaurantSessionFilter
            {
                SessionId = sessionId,
                UserId = userId
            };
            _db.RestaurantSessionFilters.Add(existing);
        }

        existing.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
        existing.District = string.IsNullOrWhiteSpace(dto.District) ? null : dto.District.Trim();
        existing.Cuisine = string.IsNullOrWhiteSpace(dto.Cuisine) ? null : dto.Cuisine.Trim();
        existing.MinRating = dto.MinRating;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{sessionId:guid}/filters/restaurants/me")]
    public async Task<ActionResult<RestaurantFiltersDto>> GetMyRestaurantFilters(Guid sessionId)
    {
        var userId = CurrentUserId;

        var f = await _db.RestaurantSessionFilters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId && x.UserId == userId);

        if (f == null) return Ok(new RestaurantFiltersDto());

        return Ok(new RestaurantFiltersDto
        {
            City = f.City,
            District = f.District,
            Cuisine = f.Cuisine,
            MinRating = f.MinRating
        });
    }

    [HttpGet("{sessionId:guid}/filters/restaurants/merged")]
    public async Task<ActionResult<RestaurantFiltersDto>> GetMergedRestaurantFilters(Guid sessionId)
    {
        var rows = await _db.RestaurantSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (rows.Count == 0) return Ok(new RestaurantFiltersDto());

        var cities = rows.Select(r => r.City).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var districts = rows.Select(r => r.District).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var cuisines = rows.Select(r => r.Cuisine).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        double? minRating = rows.Any(r => r.MinRating.HasValue)
            ? rows.Where(r => r.MinRating.HasValue).Min(r => r.MinRating)
            : null;

        return Ok(new RestaurantFiltersDto
        {
            City = cities.Count == 1 ? cities[0] : null,
            District = districts.Count == 1 ? districts[0] : null,
            Cuisine = cuisines.Count == 1 ? cuisines[0] : null,
            MinRating = minRating
        });
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
    existing.Cuisine = NormalizeCsv(dto.Cuisine);
    existing.FoodType = dto.FoodType;
    existing.BudgetLevel = null;
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
        var cuisines = rows
            .SelectMany(r => SplitCsv(r.Cuisine))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

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
            Cuisine = cuisines.Count == 0 ? null : string.Join(",", cuisines),
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

        existing.GameType = NormalizeCsv(dto.GameType);
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
            .SelectMany(r => SplitCsv(r.GameType))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

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
            GameType = gameTypes.Count == 0 ? null : string.Join(",", gameTypes),
            DurationMin = durationMin,
            DurationMax = durationMax,
            PlayersMin = playersMin,
            PlayersMax = playersMax,
            MinRating = minRating
        });
    }


    private async Task RefreshSessionStatusAsync(Guid sessionId)
    {
        var session = await _db.MatchSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session == null)
        {
            return;
        }

        if (IsTerminalSessionStatus(session.Status))
        {
            return;
        }

        var pendingInvites = await _db.SessionInvitations.CountAsync(x => x.SessionId == sessionId && x.Status == "Pending");
        var acceptedInvites = await _db.SessionInvitations.CountAsync(x => x.SessionId == sessionId && x.Status == "Accepted");
        var declinedInvites = await _db.SessionInvitations.CountAsync(x => x.SessionId == sessionId && x.Status == "Declined");

        session.Status = pendingInvites > 0
            ? "Pending"
            : acceptedInvites > 0 && declinedInvites == 0
                ? "Active"
                : acceptedInvites > 0
                    ? "Partial"
                    : "Declined";

        await _db.SaveChangesAsync();
    }

    private async Task ExpireStaleSessionAsync(Guid sessionId)
    {
        var session = await _db.MatchSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session == null || IsTerminalSessionStatus(session.Status))
        {
            return;
        }

        if (session.CreatedAtUtc > DateTime.UtcNow.AddHours(-24))
        {
            return;
        }

        session.Status = "Expired";

        var pendingInvitations = await _db.SessionInvitations
            .Where(x => x.SessionId == sessionId && x.Status == "Pending")
            .ToListAsync();

        foreach (var invitation in pendingInvitations)
        {
            invitation.Status = "Expired";
            invitation.RespondedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    private static bool IsTerminalSessionStatus(string? status)
        => string.Equals(status, "Finished", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "Closed", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "Declined", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    private async Task RefreshSessionCompletionAsync(Guid sessionId)
    {
        var session = await _db.MatchSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session == null || !string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var participantCount = await _db.MatchSessionParticipants.CountAsync(x => x.SessionId == sessionId);
        if (participantCount == 0)
        {
            session.Status = "Finished";
            await _db.SaveChangesAsync();
            return;
        }

        var candidates = await _db.SessionItems
            .Where(i => i.SessionId == sessionId)
            .Where(i => !_db.SessionMatches.Any(m => m.SessionId == sessionId && m.ItemId == i.Id))
            .OrderBy(i => i.Id)
            .ToListAsync();

        var filteredItems = await ApplyFiltersAsync(sessionId, session.Category, candidates);
        var trackedItemIds = filteredItems.Select(i => i.Id).ToHashSet();

        var votedItemIds = await _db.SwipeVotes
            .Where(v => v.SessionId == sessionId)
            .Select(v => v.ItemId)
            .Distinct()
            .ToListAsync();

        foreach (var itemId in votedItemIds)
        {
            trackedItemIds.Add(itemId);
        }

        var trackedItemIdList = trackedItemIds.ToList();
        var voteCountsByItem = await _db.SwipeVotes
            .Where(v => v.SessionId == sessionId && trackedItemIdList.Contains(v.ItemId))
            .GroupBy(v => v.ItemId)
            .Select(g => new { ItemId = g.Key, Count = g.Select(v => v.UserId).Distinct().Count() })
            .ToListAsync();

        var hasRemaining = trackedItemIdList.Any(itemId =>
        {
            var voteCount = voteCountsByItem.FirstOrDefault(v => v.ItemId == itemId)?.Count ?? 0;
            return voteCount < participantCount;
        });

        if (!hasRemaining)
        {
            session.Status = "Finished";
            await _db.SaveChangesAsync();
        }
    }

    private async Task<string> BuildFilterSummaryAsync(Guid sessionId, string category)
    {
        var normalized = DemoCatalog.NormalizeCategory(category);

        return normalized switch
        {
            "Movie" => await BuildMovieFilterSummaryAsync(sessionId),
            "Restaurant" => await BuildRestaurantFilterSummaryAsync(sessionId),
            "Recipe" => await BuildRecipeFilterSummaryAsync(sessionId),
            "BoardGame" => await BuildBoardGameFilterSummaryAsync(sessionId),
            _ => "No filters"
        };
    }

    private async Task<string> BuildMovieFilterSummaryAsync(Guid sessionId)
    {
        var rows = await _db.MovieSessionFilters.AsNoTracking().Where(x => x.SessionId == sessionId).ToListAsync();
        if (rows.Count == 0) return "No filters saved";

        var genres = rows.SelectMany(r => string.IsNullOrWhiteSpace(r.GenresCsv) ? Array.Empty<string>() : r.GenresCsv.Split(','))
            .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var minRating = rows.Any(r => r.MinRating.HasValue) ? rows.Where(r => r.MinRating.HasValue).Min(r => r.MinRating) : null;
        var yearFrom = rows.Any(r => r.YearFrom.HasValue) ? rows.Where(r => r.YearFrom.HasValue).Min(r => r.YearFrom) : null;
        var yearTo = rows.Any(r => r.YearTo.HasValue) ? rows.Where(r => r.YearTo.HasValue).Max(r => r.YearTo) : null;

        return $"Genres: {(genres.Count == 0 ? "Any" : string.Join(", ", genres))}; Rating: {(minRating.HasValue ? minRating.Value.ToString("0.0") + "+" : "Any")}; Years: {(yearFrom.HasValue || yearTo.HasValue ? $"{yearFrom?.ToString() ?? "Any"}-{yearTo?.ToString() ?? "Any"}" : "Any")}";
    }

    private async Task<string> BuildRestaurantFilterSummaryAsync(Guid sessionId)
    {
        var rows = await _db.RestaurantSessionFilters.AsNoTracking().Where(x => x.SessionId == sessionId).ToListAsync();
        if (rows.Count == 0) return "No filters saved";

        var cities = rows.Select(r => r.City).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var districts = rows.Select(r => r.District).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var cuisines = rows.Select(r => r.Cuisine).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var minRating = rows.Any(r => r.MinRating.HasValue) ? rows.Where(r => r.MinRating.HasValue).Min(r => r.MinRating) : null;

        return $"City: {(cities.Count == 1 ? cities[0] : cities.Count > 1 ? "Mixed" : "Any")}; District: {(districts.Count == 1 ? districts[0] : districts.Count > 1 ? "Mixed" : "Any")}; Cuisine: {(cuisines.Count == 1 ? cuisines[0] : cuisines.Count > 1 ? "Mixed" : "Any")}; Rating: {(minRating.HasValue ? minRating.Value.ToString("0.0") + "+" : "Any")}";
    }

    private async Task<string> BuildRecipeFilterSummaryAsync(Guid sessionId)
    {
        var rows = await _db.RecipeSessionFilters.AsNoTracking().Where(x => x.SessionId == sessionId).ToListAsync();
        if (rows.Count == 0) return "No filters saved";

        var cuisines = rows.SelectMany(r => SplitCsv(r.Cuisine)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var foodTypes = rows.Select(r => r.FoodType).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var ingredients = rows.SelectMany(r => string.IsNullOrWhiteSpace(r.IngredientsCsv) ? Array.Empty<string>() : r.IngredientsCsv.Split(','))
            .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var minRating = rows.Any(r => r.MinRating.HasValue) ? rows.Where(r => r.MinRating.HasValue).Min(r => r.MinRating) : null;
        var maxComplexity = rows.Any(r => r.Complexity.HasValue) ? rows.Where(r => r.Complexity.HasValue).Min(r => r.Complexity) : null;

        return $"Cuisine: {(cuisines.Count == 0 ? "Any" : string.Join(", ", cuisines))}; Type: {(foodTypes.Count == 1 ? foodTypes[0] : foodTypes.Count > 1 ? "Mixed" : "Any")}; Complexity <= {(maxComplexity?.ToString() ?? "Any")}; Rating: {(minRating.HasValue ? minRating.Value.ToString("0.0") + "+" : "Any")}; Ingredients: {(ingredients.Count == 0 ? "Any" : string.Join(", ", ingredients))}";
    }

    private async Task<string> BuildBoardGameFilterSummaryAsync(Guid sessionId)
    {
        var rows = await _db.BoardGameSessionFilters.AsNoTracking().Where(x => x.SessionId == sessionId).ToListAsync();
        if (rows.Count == 0) return "No filters saved";

        var gameTypes = rows.SelectMany(r => SplitCsv(r.GameType)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var playersMin = rows.Any(r => r.PlayersMin.HasValue) ? rows.Where(r => r.PlayersMin.HasValue).Min(r => r.PlayersMin) : null;
        var playersMax = rows.Any(r => r.PlayersMax.HasValue) ? rows.Where(r => r.PlayersMax.HasValue).Max(r => r.PlayersMax) : null;
        var durationMin = rows.Any(r => r.DurationMin.HasValue) ? rows.Where(r => r.DurationMin.HasValue).Min(r => r.DurationMin) : null;
        var durationMax = rows.Any(r => r.DurationMax.HasValue) ? rows.Where(r => r.DurationMax.HasValue).Max(r => r.DurationMax) : null;
        var minRating = rows.Any(r => r.MinRating.HasValue) ? rows.Where(r => r.MinRating.HasValue).Min(r => r.MinRating) : null;

        return $"Type: {(gameTypes.Count == 0 ? "Any" : string.Join(", ", gameTypes))}; Players: {(playersMin?.ToString() ?? "Any")}-{(playersMax?.ToString() ?? "Any")}; Duration: {(durationMin?.ToString() ?? "Any")}-{(durationMax?.ToString() ?? "Any")} min; Rating: {(minRating.HasValue ? minRating.Value.ToString("0.0") + "+" : "Any")}";
    }

    private static object ToItemResponse(SessionItem item)
    {
        object? meta = null;
        if (!string.IsNullOrWhiteSpace(item.MetaJson))
        {
            meta = JsonSerializer.Deserialize<JsonElement>(item.MetaJson);
        }

        return new
        {
            item.Id,
            item.Title,
            item.Category,
            item.ImageUrl,
            Meta = meta
        };
    }

    private static object ToItemResponse(dynamic item)
    {
        var itemType = ((object)item).GetType();

        string? metaJson = itemType.GetProperty("MetaJson")?.GetValue(item) as string;
        object? meta = null;
        if (!string.IsNullOrWhiteSpace(metaJson))
        {
            meta = JsonSerializer.Deserialize<JsonElement>(metaJson);
        }

        var createdAtUtc = itemType.GetProperty("CreatedAtUtc")?.GetValue(item);
        var sessionIdValue = itemType.GetProperty("SessionId")?.GetValue(item);
        Guid? sessionId = sessionIdValue is Guid parsedSessionId ? parsedSessionId : null;
        var matchedUsersValue = itemType.GetProperty("MatchedUsers")?.GetValue(item);
        var matchedUsers = matchedUsersValue is IEnumerable<string> names
            ? names.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : new List<string>();

        return new
        {
            Id = (Guid)itemType.GetProperty("Id")!.GetValue(item)!,
            Title = (string)(itemType.GetProperty("Title")!.GetValue(item) ?? string.Empty),
            Category = (string)(itemType.GetProperty("Category")!.GetValue(item) ?? string.Empty),
            ImageUrl = itemType.GetProperty("ImageUrl")?.GetValue(item) as string,
            Meta = meta,
            CreatedAtUtc = createdAtUtc,
            SessionId = sessionId,
            MatchedUsers = matchedUsers
        };
    }

    private async Task<List<SessionItem>> ApplyFiltersAsync(Guid sessionId, string category, List<SessionItem> items)
    {
        var normalized = DemoCatalog.NormalizeCategory(category);

        return normalized switch
        {
            "Movie" => await ApplyMovieFiltersAsync(sessionId, items),
            "Restaurant" => await ApplyRestaurantFiltersAsync(sessionId, items),
            "Recipe" => await ApplyRecipeFiltersAsync(sessionId, items),
            "BoardGame" => await ApplyBoardGameFiltersAsync(sessionId, items),
            _ => items
        };
    }

    private async Task<List<SessionItem>> ApplyMovieFiltersAsync(Guid sessionId, List<SessionItem> items)
    {
        var filter = await _db.MovieSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (filter.Count == 0) return items;

        double? minRating = filter.Any(x => x.MinRating.HasValue)
            ? filter.Where(x => x.MinRating.HasValue).Min(x => x.MinRating)
            : null;
        int? yearFrom = filter.Any(x => x.YearFrom.HasValue)
            ? filter.Where(x => x.YearFrom.HasValue).Min(x => x.YearFrom)
            : null;
        int? yearTo = filter.Any(x => x.YearTo.HasValue)
            ? filter.Where(x => x.YearTo.HasValue).Max(x => x.YearTo)
            : null;
        var genres = filter
            .SelectMany(x => string.IsNullOrWhiteSpace(x.GenresCsv) ? [] : x.GenresCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return items.Where(item =>
        {
            var meta = ParseMeta(item);
            var rating = GetDouble(meta, "rating");
            var year = GetInt(meta, "year");
            var itemGenres = GetStrings(meta, "genres");

            return (!minRating.HasValue || rating >= minRating.Value)
                   && (!yearFrom.HasValue || year >= yearFrom.Value)
                   && (!yearTo.HasValue || year <= yearTo.Value)
                   && (genres.Count == 0 || itemGenres.Any(g => genres.Contains(g)));
        }).ToList();
    }

    private async Task<List<SessionItem>> ApplyRestaurantFiltersAsync(Guid sessionId, List<SessionItem> items)
    {
        var filter = await _db.RestaurantSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (filter.Count == 0) return items;

        var city = Mode(filter.Select(x => x.City));
        var district = Mode(filter.Select(x => x.District));
        double? minRating = filter.Any(x => x.MinRating.HasValue)
            ? filter.Where(x => x.MinRating.HasValue).Min(x => x.MinRating)
            : null;
        var cuisines = filter.Select(x => x.Cuisine).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var cuisine = cuisines.Count == 1 ? cuisines[0] : null;

        return items.Where(item =>
        {
            var meta = ParseMeta(item);
            var rating = GetDouble(meta, "rating");
            var itemCity = GetString(meta, "city");
            var itemDistrict = GetString(meta, "district");
            var itemCuisine = GetString(meta, "cuisine");

            return (string.IsNullOrWhiteSpace(city) || string.Equals(itemCity, city, StringComparison.OrdinalIgnoreCase))
                   && (string.IsNullOrWhiteSpace(district) || string.Equals(itemDistrict, district, StringComparison.OrdinalIgnoreCase))
                   && (cuisines.Count == 0 || (!string.IsNullOrWhiteSpace(itemCuisine) && cuisines.Contains(itemCuisine)))
                   && (!minRating.HasValue || rating >= minRating.Value);
        }).ToList();
    }

    private async Task<List<SessionItem>> ApplyRecipeFiltersAsync(Guid sessionId, List<SessionItem> items)
    {
        var filter = await _db.RecipeSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (filter.Count == 0) return items;

        int? complexity = filter.Any(x => x.Complexity.HasValue)
            ? filter.Where(x => x.Complexity.HasValue).Min(x => x.Complexity)
            : null;
        double? minRating = filter.Any(x => x.MinRating.HasValue)
            ? filter.Where(x => x.MinRating.HasValue).Min(x => x.MinRating)
            : null;
        var cuisines = filter
            .SelectMany(x => SplitCsv(x.Cuisine))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var foodType = SingleOrNull(filter.Select(x => x.FoodType));
        var ingredients = filter
            .SelectMany(x => string.IsNullOrWhiteSpace(x.IngredientsCsv) ? [] : x.IngredientsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return items.Where(item =>
        {
            var meta = ParseMeta(item);
            var itemComplexity = GetInt(meta, "complexity");
            var rating = GetDouble(meta, "rating");
            var itemCuisine = GetString(meta, "cuisine");
            var itemFoodType = GetString(meta, "foodType");
            var itemIngredients = GetStrings(meta, "ingredients");

            return (!complexity.HasValue || itemComplexity <= complexity.Value)
                   && (!minRating.HasValue || rating >= minRating.Value)
                   && (cuisines.Count == 0 || (!string.IsNullOrWhiteSpace(itemCuisine) && cuisines.Contains(itemCuisine)))
                   && (string.IsNullOrWhiteSpace(foodType) || string.Equals(itemFoodType, foodType, StringComparison.OrdinalIgnoreCase))
                   && (ingredients.Count == 0 || ingredients.Any(i => itemIngredients.Contains(i, StringComparer.OrdinalIgnoreCase)));
        }).ToList();
    }

    private async Task<List<SessionItem>> ApplyBoardGameFiltersAsync(Guid sessionId, List<SessionItem> items)
    {
        var filter = await _db.BoardGameSessionFilters
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId)
            .ToListAsync();

        if (filter.Count == 0) return items;

        var gameTypes = filter
            .SelectMany(x => SplitCsv(x.GameType))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int? durationMin = filter.Any(x => x.DurationMin.HasValue)
            ? filter.Where(x => x.DurationMin.HasValue).Min(x => x.DurationMin)
            : null;
        int? durationMax = filter.Any(x => x.DurationMax.HasValue)
            ? filter.Where(x => x.DurationMax.HasValue).Max(x => x.DurationMax)
            : null;
        int? playersMin = filter.Any(x => x.PlayersMin.HasValue)
            ? filter.Where(x => x.PlayersMin.HasValue).Min(x => x.PlayersMin)
            : null;
        int? playersMax = filter.Any(x => x.PlayersMax.HasValue)
            ? filter.Where(x => x.PlayersMax.HasValue).Max(x => x.PlayersMax)
            : null;
        double? minRating = filter.Any(x => x.MinRating.HasValue)
            ? filter.Where(x => x.MinRating.HasValue).Min(x => x.MinRating)
            : null;

        return items.Where(item =>
        {
            var meta = ParseMeta(item);
            var itemType = GetString(meta, "gameType");
            var itemDurationMin = GetInt(meta, "durationMin");
            var itemDurationMax = GetInt(meta, "durationMax");
            var itemPlayersMin = GetInt(meta, "playersMin");
            var itemPlayersMax = GetInt(meta, "playersMax");
            var rating = GetDouble(meta, "rating");

            return (gameTypes.Count == 0 || (!string.IsNullOrWhiteSpace(itemType) && gameTypes.Contains(itemType)))
                   && (!durationMin.HasValue || itemDurationMin >= durationMin.Value)
                   && (!durationMax.HasValue || itemDurationMax <= durationMax.Value)
                   && (!playersMin.HasValue || itemPlayersMin >= playersMin.Value)
                   && (!playersMax.HasValue || itemPlayersMax <= playersMax.Value)
                   && (!minRating.HasValue || rating >= minRating.Value);
        }).ToList();
    }

    private static JsonElement ParseMeta(SessionItem item)
        => string.IsNullOrWhiteSpace(item.MetaJson)
            ? default
            : JsonSerializer.Deserialize<JsonElement>(item.MetaJson);

    private static string? GetString(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value)
            ? value.GetString()
            : null;

    private static int GetInt(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.TryGetInt32(out var number)
            ? number
            : 0;

    private static double GetDouble(JsonElement meta, string property)
        => meta.ValueKind == JsonValueKind.Object && meta.TryGetProperty(property, out var value) && value.TryGetDouble(out var number)
            ? number
            : 0;

    private static List<string> GetStrings(JsonElement meta, string property)
    {
        if (meta.ValueKind != JsonValueKind.Object || !meta.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
            return [];

        return value.EnumerateArray()
            .Where(x => x.ValueKind == JsonValueKind.String)
            .Select(x => x.GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToList();
    }


    private static string? NormalizeCsv(string? value)
    {
        var values = SplitCsv(value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        return values.Count == 0 ? null : string.Join(",", values);
    }

    private static IEnumerable<string> SplitCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x));
    private static string? SingleOrNull(IEnumerable<string?> values)
    {
        var distinct = values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return distinct.Count == 1 ? distinct[0] : null;
    }

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









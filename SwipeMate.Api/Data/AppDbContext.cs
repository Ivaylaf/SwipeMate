using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SwipeMate.Api.Models;

namespace SwipeMate.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FriendshipRequest> FriendshipRequests => Set<FriendshipRequest>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<MatchSession> MatchSessions => Set<MatchSession>();
    public DbSet<MatchSessionParticipant> MatchSessionParticipants => Set<MatchSessionParticipant>();
    public DbSet<SessionInvitation> SessionInvitations => Set<SessionInvitation>();
    public DbSet<SessionItem> SessionItems => Set<SessionItem>();
    public DbSet<SwipeVote> SwipeVotes => Set<SwipeVote>();
    public DbSet<SessionMatch> SessionMatches => Set<SessionMatch>();
    public DbSet<SessionFilter> SessionFilters => Set<SessionFilter>();
    public DbSet<MovieSessionFilter> MovieSessionFilters => Set<MovieSessionFilter>();
    public DbSet<RestaurantSessionFilter> RestaurantSessionFilters => Set<RestaurantSessionFilter>();
    public DbSet<RecipeSessionFilter> RecipeSessionFilters => Set<RecipeSessionFilter>();
    public DbSet<BoardGameSessionFilter> BoardGameSessionFilters => Set<BoardGameSessionFilter>();
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
}

using Microsoft.AspNetCore.Identity;

namespace SwipeMate.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedAtUtc { get; set; }
    public string? BlockedReason { get; set; }
}



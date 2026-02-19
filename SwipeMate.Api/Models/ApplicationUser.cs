using Microsoft.AspNetCore.Identity;

namespace SwipeMate.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}


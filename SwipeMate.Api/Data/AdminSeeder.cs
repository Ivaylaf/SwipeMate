using Microsoft.AspNetCore.Identity;
using SwipeMate.Api.Models;

namespace SwipeMate.Api.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        const string adminRole = "Admin";
        const string userRole = "User";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        if (!await roleManager.RoleExistsAsync(userRole))
        {
            await roleManager.CreateAsync(new IdentityRole(userRole));
        }

        var adminUserName = config["AdminSeed:UserName"] ?? "admin";
        var adminEmail = config["AdminSeed:Email"] ?? "admin@swipemate.bg";
        var adminDisplayName = config["AdminSeed:DisplayName"] ?? "SwipeMate Admin";
        var adminPassword = config["AdminSeed:Password"] ?? "admin123";

        var admin = await userManager.FindByNameAsync(adminUserName)
            ?? await userManager.FindByEmailAsync(adminEmail);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                DisplayName = adminDisplayName,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Could not create admin user: " + string.Join(", ", createResult.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            await userManager.AddToRoleAsync(admin, adminRole);
        }
    }
}

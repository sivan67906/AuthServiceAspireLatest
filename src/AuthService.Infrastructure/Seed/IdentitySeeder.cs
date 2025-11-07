using AuthService.Domain.Users;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthService.Infrastructure.Seed;

public static class IdentitySeeder
{
    public static async Task InitializeDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var write = scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        await write.Database.MigrateAsync();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        string[] roles = ["Admin", "User"];
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole<Guid>(r));
        var adminEmail = "admin@demo.local";
        var admin = await userManager.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (admin is null)
        {
            admin = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, FirstName = "System", LastName = "Admin" };
            var result = await userManager.CreateAsync(admin, "Admin@12345");
            if (result.Succeeded)
                await userManager.AddToRolesAsync(admin, roles);
        }
    }
}
using Microsoft.AspNetCore.Identity;

namespace CapheVanPhong.Infrastructure.Seeding;

public class DatabaseSeeder
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DatabaseSeeder(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        // Ensure both roles exist (belt-and-suspenders alongside the migration seed)
        string[] roles = ["superadmin", "admin", "user"];
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedUsersAsync()
    {
        string password = "Password@123";
        // Seed User account
        const string userEmail = "user@caphevanphong.vn";

        if (await _userManager.FindByEmailAsync(userEmail) is not null)
            return;

        var user = new IdentityUser
        {
            UserName = "User",
            Email = userEmail,
            EmailConfirmed = true
        };

        var result1 = await _userManager.CreateAsync(user, password);
        if (result1.Succeeded)
            await _userManager.AddToRoleAsync(user, "User");

        // Seed Admin account
        const string adminEmail = "admin@caphevanphong.vn";

        if (await _userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new IdentityUser
        {
            UserName = "Admin",
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result2 = await _userManager.CreateAsync(admin, password);
        if (result2.Succeeded)
            await _userManager.AddToRoleAsync(admin, "Admin");


        // Seed SuperAdmin account
        const string sadminEmail = "superadmin@caphevanphong.vn";

        if (await _userManager.FindByEmailAsync(sadminEmail) is not null)
            return;

        var sadmin = new IdentityUser
        {
            UserName = "SuperAdmin",
            Email = sadminEmail,
            EmailConfirmed = true
        };

        var result3 = await _userManager.CreateAsync(sadmin, password);
        if (result3.Succeeded)
            await _userManager.AddToRoleAsync(sadmin, "SuperAdmin");
    }
}

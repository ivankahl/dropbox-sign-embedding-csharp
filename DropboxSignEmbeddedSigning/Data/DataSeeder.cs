using Microsoft.AspNetCore.Identity;

namespace DropboxSignEmbeddedSigning.Data;

public class DataSeeder(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    public async Task SeedDataAsync()
    {
        // Seed roles
        const string adminRoleName = "Administrator";
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }
        
        // Seed users
        const string adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                Id = "e2250851-41a6-4b97-a4ef-eb99283c07db",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };  
            
            await userManager.CreateAsync(adminUser, "Password123!");
        }
        
        // Seed adding user to role
        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
}
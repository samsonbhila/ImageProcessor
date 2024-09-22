using ImageProcessor.Enums;
using Microsoft.AspNetCore.Identity;

namespace ImageProcessor.Seeder;

public class RoleTableSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public RoleTableSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }
    
    public async Task SeedRolesAsync()
    {
        string[] roleNames =
        {
            AppRoles.IntegratedSystem.ToString(), 
        };

        foreach (var roleName in roleNames)
        {
            if (!await _roleManager.RoleExistsAsync(roleName)) {

                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                
                if (!roleResult.Succeeded) {
                    throw new Exception($"Failed to create role {roleName}");
                }
            }
        }
    }
}
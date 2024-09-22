using ImageProcessor.Enums;
using ImageProcessor.Models;
using Microsoft.AspNetCore.Identity;

namespace ImageProcessor.Seeder;

public class UserTableSeeder
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    
    public UserTableSeeder(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }
    
        public async Task SeedAdministratorsAsync()
    {
        Dictionary<String, Dictionary<String, String>> userData = new Dictionary<String, Dictionary<String, String>>();

        userData.Add("sifisoshishaba@outlook.com", new Dictionary<string, string>
        {
            { "password", _configuration["DefaultPassword"] },
            { "role", nameof(AppRoles.IntegratedSystem)}
        });

        foreach (var data in userData)
        {
            var user = new AppUser
            {
                UserName = data.Key, 
                Email = data.Key,
                EmailConfirmed = true,
            };
            
            var userResult = await _userManager.CreateAsync(user, data.Value["password"]);

            if (userResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, data.Value["role"]);
            }
        }
    }
}
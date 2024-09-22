using System.Text;
using ImageProcessor.Enums;
using ImageProcessor.Models;
using System.Security.Claims;
using ImageProcessor.Exceptions;
using ImageProcessor.Dto.Request;
using ImageProcessor.Dto.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using ImageProcessor.Services.Contracts;

namespace ImageProcessor.Services.Implementation;

public class AuthService: IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(ILogger<AuthService> logger, UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _logger = logger;
        _userManager = userManager;
        _configuration = configuration;
    }
    public async Task<AppUser?> GetUserByEmail(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }
    public async Task<LoginResponse> Login(LoginRequest request)
    { 
        AppUser? user = await GetUserByEmail(request.Email);

        if (user != null && await _userManager.CheckPasswordAsync(user, request.Password))
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var securityKey = _configuration
                .GetSection(ConfigConst.Authentication.ToString())
                .GetSection(ConfigConst.SecurityKey.ToString()).Value;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: ConfigConst.ImageProcessorApi.ToString(),
                claims: claims,
                expires: DateTime.Now.AddDays(365),
                signingCredentials: creds);

            return new LoginResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Expiration = jwtSecurityToken.ValidTo
            };
        }

        _logger.LogWarning($"The provided credentials for { request.Email } do not match our records");
        throw new GenericUserException("The provided credentials do not match our records");
    }
}
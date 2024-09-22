using ImageProcessor.Dto.Request;
using ImageProcessor.Dto.Response;
using ImageProcessor.Exceptions;
using ImageProcessor.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ImageProcessor.Controllers;

[ApiController]
[Route("api/v1/identity")]
public class IdentityController : ControllerBase
{
    private readonly IAuthService _authService;

    public IdentityController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Auth([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _authService.Login(request));
        }
        catch (GenericUserException e)
        {
            return Unauthorized(e.Message);
        }
    }
}
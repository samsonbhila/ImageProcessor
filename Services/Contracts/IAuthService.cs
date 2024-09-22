using ImageProcessor.Dto.Request;
using ImageProcessor.Dto.Response;

namespace ImageProcessor.Services.Contracts;

public interface IAuthService
{
    public Task<LoginResponse> Login(LoginRequest request);
}
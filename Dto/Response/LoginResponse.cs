using System;

namespace ImageProcessor.Dto.Response;

public record LoginResponse
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}
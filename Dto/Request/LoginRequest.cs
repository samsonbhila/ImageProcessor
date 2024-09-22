using System.ComponentModel.DataAnnotations;

namespace ImageProcessor.Dto.Request;

public record LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }
}
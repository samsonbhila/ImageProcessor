using Microsoft.AspNetCore.Mvc;
using ImageProcessor.Dto.Request;
using ImageProcessor.Dto.Response;
using ImageProcessor.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace ImageProcessor.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/image")]
public class ImageController : ControllerBase
{
    private readonly IimageService _imageService;

    public ImageController(IimageService imageService)
    {
        _imageService = imageService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessOrder([FromBody] ProcessOrderRequest request)
    {
        return Ok(await _imageService.ProcessOrder(request));
    }

    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(QueueStatus), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus([FromRoute] string Id)
    {
        return Ok(await _imageService.GetQueueStatus(Id));
    }
}
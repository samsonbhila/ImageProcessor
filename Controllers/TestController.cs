using ImageProcessor.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom.Compiler;
using System.Threading.Tasks;
using System;

namespace ImageProcessor.Controllers;

[ApiController]
[Route("api/v1/test")]
public class TestController : ControllerBase
{
    private readonly ICodeGenerator _codeGenerator;
    private readonly IimageService _imageService;

    public TestController(ICodeGenerator codeGenerator, IimageService imageService)
    {
        _codeGenerator = codeGenerator;
        _imageService = imageService;
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        string guid = Guid.NewGuid().ToString();
        await _codeGenerator.GenerateQrCode(5365678969656, guid);
        await _codeGenerator.GenerateBarCode(8965678857656, guid);
        await _imageService.GenerateCardFront(guid, "Temp/56b4f1a3-dfc9-5526-b344-b4f6c3de91af.jpeg", "Investment", "Platinum", DateTime.Now.AddYears(2));
        return Ok();
    }
}
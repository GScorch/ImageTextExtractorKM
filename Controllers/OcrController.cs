using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly OcrService _ocrService;

    public OcrController(OcrService ocrService)
    {
        _ocrService = ocrService;
    }

    // [HttpPost("extract-text")]
    // public string ExtractText(Stream image, string dir, string filename) //IFormFile awalnya
    // {
    //     if (image == null || image.Length == 0)
    //         return ("Image file is required.");

    //     //var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    //     // if (!Directory.Exists(uploadFolder))
    //     // {
    //     //     Directory.CreateDirectory(uploadFolder);
    //     // }
    //     // var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
    //     // var filePath = Path.Combine(uploadFolder, fileName);
    //     // Save the file to the directory
    //     // using (var fileStream = new FileStream(filePath, FileMode.Create))
    //     // {
    //     //     image.CopyTo(fileStream);
    //     // }

    //     // Process the file to extract text
    //     //using var stream = image.OpenReadStream();
    //     //var text = _ocrService.ExtractText(stream);
    //     var text = _ocrService.ExtractText(image, dir, filename);
    //     return (text);
    // }
// public IActionResult ExtractText(Stream image, string dir, string filename) //IFormFile awalnya
//     {
//         if (image == null || image.Length == 0)
//             return BadRequest("Image file is required.");

//         //var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
//         // if (!Directory.Exists(uploadFolder))
//         // {
//         //     Directory.CreateDirectory(uploadFolder);
//         // }
//         // var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
//         // var filePath = Path.Combine(uploadFolder, fileName);
//         // Save the file to the directory
//         // using (var fileStream = new FileStream(filePath, FileMode.Create))
//         // {
//         //     image.CopyTo(fileStream);
//         // }

//         // Process the file to extract text
//         //using var stream = image.OpenReadStream();
//         //var text = _ocrService.ExtractText(stream);
//         var text = _ocrService.ExtractText(image, dir, filename);
//         return Ok(new { text });
//     }
    // [HttpPost("debug-preview")]
    // public IActionResult DebugPreview([FromForm] IFormFile image)
    // {
    //     using var stream = image.OpenReadStream();
    //     var result = _ocrService.PreprocessImage(stream);
    //     return File(result, "image/png");
    // }


}

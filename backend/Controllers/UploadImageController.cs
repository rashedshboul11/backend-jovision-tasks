using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers; 
[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    
    [HttpPost("upload-image")]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromForm] string ownerName)
    {
        try
        {
            // Validate file
            if (file.Length == 0)
                return BadRequest("No file uploaded.");
            
            if (string.IsNullOrWhiteSpace(ownerName))
                return BadRequest("Owner name is required.");

            // Validate file type 
            if (Path.GetExtension(file.FileName).ToLower() != ".jpg")
            {
                return BadRequest("Only jpg files are supported.");
            }
            
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var metaDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/metadata");
            
            //create directory if it doesn’t already exist
            Directory.CreateDirectory(uploadsFolder);
            Directory.CreateDirectory(metaDataFolder);
            
            // 
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var fileName = $"{ownerName}_{timestamp}.jpg";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var metadata = new 
            {
                FileName = fileName,
                OwnerName = ownerName,
                UploadedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                FileSize = file.Length
            };
            var metadataJson = JsonSerializer.Serialize(metadata , new JsonSerializerOptions { WriteIndented = true});
            var metadataFilePath = Path.Combine(metaDataFolder, $"{Path.GetFileNameWithoutExtension(fileName)}_metadata.json");
            await System.IO.File.WriteAllTextAsync(metadataFilePath, metadataJson);

    
            return Ok(new { Message = "File uploaded successfully", FileName = fileName, OwnerName = ownerName });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
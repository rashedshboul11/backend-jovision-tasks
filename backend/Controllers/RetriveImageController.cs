using System.Text.Json;
using backend.models;
using Microsoft.AspNetCore.Mvc;


namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RetriveImageController : Controller
{
    [HttpGet("retrieve-image")]
    public async Task<IActionResult> RetrieveImage([FromQuery] string fileName, [FromQuery] string ownerName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name is required");
            if (string.IsNullOrEmpty(ownerName))
                return BadRequest("Owner name is required");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var metaDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/metadata");
        
            if (!Directory.Exists(metaDataFolder) || !Directory.Exists(uploadsFolder))
                return StatusCode(500, "Server folders not found");
            
            var metadataFiles = Directory.GetFiles(metaDataFolder, "*.json");

            string? actualFileName = null;
            foreach (var metadataFile in metadataFiles)
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(metadataFile);
                var metadata = JsonSerializer.Deserialize<Metadata>(jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (metadata != null && metadata.OwnerName == ownerName && metadata.FileName == fileName)
                {
                    actualFileName = metadata.FileName;
                    break;
                }
            }
        
            if (actualFileName == null)
                return NotFound("No matching file found for the owner and filename.");
            
            var filePath = Path.Combine(uploadsFolder, actualFileName);
        
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on server.");
            
            byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(imageBytes, "image/jpg", fileName);
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Internal server error: {e.Message}");
        }
    }
    
}
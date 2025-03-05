using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using backend.models;
namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeleteImageController : Controller
{
    [HttpGet("delete-image")]
    public IActionResult DeleteImage([FromQuery] string owner, [FromQuery] string imageName) 
    {
        try
        {
            if (string.IsNullOrEmpty(imageName) || string.IsNullOrEmpty(owner))
            {
                return BadRequest("Owner and image name are required.");
            }
            var metadataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/metadata");
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(metadataFolder))
            {
                return NotFound("Metadata folder not found.");
            }

            var metadataFiles = Directory.GetFiles(metadataFolder, "*.json");
            bool fileDeleted = false;

            foreach (var metadataFile in metadataFiles)
            {
                var jsonContent = System.IO.File.ReadAllText(metadataFile);
                var metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (metadata != null && metadata.OwnerName == owner && metadata.FileName == imageName)
                {
                    // Delete Image File
                    var imagePath = Path.Combine(uploadsFolder, imageName);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    // Delete Metadata File
                    System.IO.File.Delete(metadataFile);

                    fileDeleted = true;
                    break;
                }
            }
            return fileDeleted ? Ok("Image and metadata deleted successfully.") : NotFound("Image not found.");
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting image: {ex.Message}");
        }
    }
}
using System.Text.Json;
using backend.models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UpdateImageController : ControllerBase
{
    [HttpPut("update-image")]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    public async Task<IActionResult> UpdateImage(IFormFile file, [FromForm] string ownerName)
    {
        try
        {
            // Validate input
            if (file == null || file.Length == 0) 
                return BadRequest("No file uploaded.");
            if (string.IsNullOrWhiteSpace(ownerName))
                return BadRequest("Owner name is required.");
            
            // Validate file type
            if (Path.GetExtension(file.FileName).ToLower() != ".jpg")
                return BadRequest("Only JPG files are supported.");
            
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var metaDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/metadata");

            if (!Directory.Exists(metaDataFolder)) 
                return StatusCode(500, "Metadata directory is missing.");
            
            var metadataFiles = Directory.GetFiles(metaDataFolder, "*.json");
            string? existingFileName = null;
            string? metadataFilePath = null;
            Metadata? updatedMetadata = null;
            
            foreach (var metadataFile in metadataFiles)
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(metadataFile);
                Metadata? metadata = null;
                
                metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                if (metadata != null && metadata.OwnerName == ownerName)
                {
                    existingFileName = metadata.FileName;
                    metadataFilePath = metadataFile;
                    updatedMetadata = metadata;
                    break;
                }
            }
            if (existingFileName == null)
                return NotFound("No matching file found for the owner.");

            var existingFilePath = Path.Combine(uploadsFolder, existingFileName);

            if (!System.IO.File.Exists(existingFilePath))
                return NotFound("The image file does not exist.");

            // Overwrite the existing file
            using (var stream = new FileStream(existingFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // Update metadata
            if (metadataFilePath != null && updatedMetadata != null)
            {
                updatedMetadata.FileSize = file.Length;
                updatedMetadata.ModifiedAt = DateTime.UtcNow; // Use UTC
                
                var metadataJson = JsonSerializer.Serialize(updatedMetadata, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(metadataFilePath, metadataJson);
            }
            return Ok("Image updated successfully.");
        }
        catch (Exception e)
        {
            return StatusCode(500, $"Internal Server Error: {e.Message}");
        }
    }
}

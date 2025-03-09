using System.Text.Json;
using backend.models;
using Microsoft.AspNetCore.Http;  
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;  
[ApiController]
[Route("api/[controller]")]  
public class TransferOwnershipController : ControllerBase  
{
    [HttpGet("transfer-owner")]  
    public async Task<IActionResult> TransferOwnership([FromQuery] string oldOwner, [FromQuery] string newOwner) 
    {
        try
        {
            if (string.IsNullOrEmpty(oldOwner) || string.IsNullOrEmpty(newOwner))
                return BadRequest("Both oldOwner and newOwner parameters are required.");  

            List<Metadata> allFiles = new List<Metadata>();
            var metaDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/metadata");
            
            if (!Directory.Exists(metaDataFolder))
                return StatusCode(500, "Metadata directory not found");

            var metadataFiles = Directory.GetFiles(metaDataFolder, "*.json");
            foreach (var file in metadataFiles)
            {
                var jsonContent = await System.IO.File.ReadAllTextAsync(file);
                Metadata? metadata = null;
                
                metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (metadata != null && metadata.OwnerName == newOwner)
                {
                    allFiles.Add(metadata);
                } 
                else if (metadata != null && metadata.OwnerName == oldOwner)
                {
                    metadata.OwnerName = newOwner;
                    
                    var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
                    { 
                        WriteIndented = true
                    });
                    
                    await System.IO.File.WriteAllTextAsync(file, updatedJson);
                    allFiles.Add(metadata);
                    
                }
            }
            
            

            return Ok(allFiles);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, "An error occurred while transferring ownership: " + e.Message);  
        }
    } 
}
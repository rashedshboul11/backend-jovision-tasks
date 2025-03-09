using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using backend.models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;



[ApiController]
[Route("api/[controller]")]
public class FilterController : ControllerBase
{
    private readonly MetadataFilterService _metadataFilterService;

    public FilterController()
    {
        _metadataFilterService = new MetadataFilterService(); // Instantiate the filter service
    }

    [HttpPost("filter-images")]
    public async Task<IActionResult> ProcessFilter([FromForm] string? creationDate, 
        [FromForm] string? modificationDate, [FromForm] FilterType filterType,[FromForm] string ownerName )
    {
        try
        {
            if (string.IsNullOrEmpty(ownerName))
                return BadRequest();

            DateTime? parsedCreationDate = null; 
            DateTime? parsedModificationDate = null;
            if (!string.IsNullOrEmpty(creationDate))
            {
                if (DateTime.TryParseExact(creationDate, "yyyy-MM-dd",
                        null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    parsedCreationDate = parsedDate;
                    Console.WriteLine(parsedDate.ToShortDateString());
                }
                else
                    return BadRequest("Invalid creation date format. Please use YYYY-MM-DD."); 
            }
            if (!string.IsNullOrEmpty(modificationDate))
            {
                if (DateTime.TryParseExact(modificationDate, "yyyy-MM-dd", 
                        null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    parsedModificationDate = parsedDate;
                    Console.WriteLine(parsedDate.ToShortDateString());
                }
                else
                {
                    return BadRequest("Invalid modification date format. Please use YYYY-MM-DD.");
                }
            }

            List<FilteredMetadata> filteredMetadatas = new();
            switch (filterType)
            {
                case FilterType.ByModificationDate:
                    filteredMetadatas = await _metadataFilterService
                        .FilterByModificationDateAsync(ownerName, parsedModificationDate); 
                    Console.WriteLine("Filtering by Modification Date");
                    break;
                case FilterType.ByCreationDateDescending:
                    Console.WriteLine("Filtering by creation date (Descending).");
                    filteredMetadatas = await _metadataFilterService.FilterByCreationDateDescendingAsync(ownerName, parsedCreationDate);
                    break;
                case FilterType.ByCreationDateAscending:
                    filteredMetadatas = await _metadataFilterService.FilterByCreationDateAscendingAsync(ownerName, parsedCreationDate);
                    Console.WriteLine("Filtering by creation date (Ascending).");
                    break;
                case FilterType.ByOwner:
                    Console.WriteLine("Filtering by owner.");
                    filteredMetadatas = await _metadataFilterService.FilterByOwnerAsync(ownerName);
                    break;
                default:
                    return BadRequest("Invalid filter type.");
            }

            return filteredMetadatas != null ? Ok(filteredMetadatas) : BadRequest();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

   

  
}
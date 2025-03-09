using System.Text.Json;
using backend.models;

namespace backend.Services;

public class MetadataFilterService
{
    private readonly string _metadataFolder;

    public MetadataFilterService()
    {
        _metadataFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/metadata");
    }

    public async Task<List<FilteredMetadata>> FilterByOwnerAsync(string ownerName)
    {
        if (!Directory.Exists(_metadataFolder))
            return new List<FilteredMetadata>(); // Return empty if directory is missing

        var metadataFiles = Directory.GetFiles(_metadataFolder, "*.json");
        var filteredResults = new List<FilteredMetadata>();

        foreach (var file in metadataFiles)
        {
            var jsonContent = await System.IO.File.ReadAllTextAsync(file);
            var metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (metadata != null && metadata.OwnerName.Equals(ownerName, StringComparison.OrdinalIgnoreCase))
            {
                filteredResults.Add(new FilteredMetadata
                {
                    FileName = metadata.FileName,
                    OwnerName = metadata.OwnerName
                });
            }
        }

        return filteredResults;
    }
    public async Task<List<FilteredMetadata>> FilterByModificationDateAsync(string owner,DateTime? modificationDate)
    {
        if (!Directory.Exists(_metadataFolder))
            return new List<FilteredMetadata>(); // Return empty list if directory is missing

        var metadataFiles = Directory.GetFiles(_metadataFolder, "*.json");
        var filteredResults = new List<FilteredMetadata>();

        foreach (var file in metadataFiles)
        {
            var jsonContent = await System.IO.File.ReadAllTextAsync(file);
            var metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (metadata != null 
                && metadata.ModifiedAt < modificationDate
                && metadata.OwnerName.Equals(owner, StringComparison.OrdinalIgnoreCase))
            {
                filteredResults.Add(new FilteredMetadata
                {
                    FileName = metadata.FileName,
                    OwnerName = metadata.OwnerName
                });
            }
        }

        return filteredResults;
    }
    public async Task<List<FilteredMetadata>> FilterByCreationDateDescendingAsync(string owner, DateTime? creationDate)
    {
        if (!Directory.Exists(_metadataFolder))
            return new List<FilteredMetadata>(); // Return empty list if directory is missing

        var metadataFiles = Directory.GetFiles(_metadataFolder, "*.json");
        var allResults = new List<(FilteredMetadata Metadata, DateTime UploadDate)>();

        foreach (var file in metadataFiles)
        {
            var jsonContent = await System.IO.File.ReadAllTextAsync(file);
            var metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Check if the file was created after the specified creation date
            // If creationDate is null, include all files for the specified owner
            if (metadata != null 
                && (creationDate == null || metadata.UploadedAt > creationDate)
                && metadata.OwnerName.Equals(owner, StringComparison.OrdinalIgnoreCase))
            {
                allResults.Add((
                    new FilteredMetadata
                    {
                        FileName = metadata.FileName,
                        OwnerName = metadata.OwnerName
                    }, 
                    metadata.UploadedAt));
            }
        }

        // Sort the results in descending order by upload date (most recent first)
        return allResults
            .OrderByDescending(item => item.UploadDate)
            .Select(item => item.Metadata)
            .ToList();
    }
// Helper method to extract creation date from filename (if your metadata doesn't already have this info)
private DateTime? GetCreationDateFromFileName(string fileName)
{
    try
    {
        var filePath = Path.Combine(_metadataFolder, fileName + ".json");
        if (File.Exists(filePath))
        {
            var jsonContent = File.ReadAllText(filePath);
            var metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return metadata?.UploadedAt;
        }
        return null;
    }
    catch
    {
        return null;
    }
}

public async Task<List<FilteredMetadata>> FilterByCreationDateAscendingAsync(string owner, DateTime? creationDate)
{
    if (!Directory.Exists(_metadataFolder))
        return new List<FilteredMetadata>(); // Return empty list if directory is missing

    var metadataFiles = Directory.GetFiles(_metadataFolder, "*.json");
    var allResults = new List<(FilteredMetadata Metadata, DateTime UploadDate)>();

    foreach (var file in metadataFiles)
    {
        var jsonContent = await System.IO.File.ReadAllTextAsync(file);
        var metadata = JsonSerializer.Deserialize<Metadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Check if the file was created after the specified creation date
        // If creationDate is null, include all files for the specified owner
        if (metadata != null 
            && (creationDate == null || metadata.UploadedAt > creationDate)
            && metadata.OwnerName.Equals(owner, StringComparison.OrdinalIgnoreCase))
        {
            allResults.Add((
                new FilteredMetadata
                {
                    FileName = metadata.FileName,
                    OwnerName = metadata.OwnerName
                }, 
                metadata.UploadedAt));
        }
    }

    // Sort the results in ascending order by upload date (oldest first)
    return allResults
        .OrderBy(item => item.UploadDate)
        .Select(item => item.Metadata)
        .ToList();
}


}
namespace backend.models;

public class Metadata
{
    public string FileName { get; set; }
    public string OwnerName { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public long FileSize { get; set; }
}

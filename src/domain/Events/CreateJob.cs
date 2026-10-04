namespace domain.Events;

public class CreateJob : BaseEvent
{
    public string FileName { get; set; }
    public Guid MediaAssetId { get; set; }

    public CreateJob(string fileName, Guid mediaAssetId)
    {
        FileName = fileName;
        MediaAssetId = mediaAssetId;
    }
}
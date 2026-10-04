using domain.Events;
using domain.Extensions;

namespace domain.Models.Jobs;

public class MediaAsset : BaseEntity
{
    public string BaseName { get; set; } = string.Empty;
    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public MediaAsset(string fileName)
    {
        Id = Guid.NewGuid();
        BaseName = fileName.CreateAssetIdentificator();
        AddEvent(new CreateJob(fileName, Id));
    }

    private MediaAsset() { }
}

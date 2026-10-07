using domain.Events;

namespace domain.Models.Jobs;

public class MediaPackage : BaseEntity
{
    protected MediaPackage() { }

    public string Identifier { get; set; } = string.Empty;
    public HealthState State { get; set; } = HealthState.Health;
    public ICollection<MediaAsset> Assets { get; set; } = new List<MediaAsset>();

    public MediaPackage(string identifier)
    {
        Id = Guid.NewGuid();
        Identifier = identifier;
    }

    public void ValidateAsset(string filename)
    {
        AddEvent(new ValidateMediaAsset(filename));
    }
}

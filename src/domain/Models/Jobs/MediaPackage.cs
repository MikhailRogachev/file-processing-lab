using domain.Events;

namespace domain.Models.Jobs;

public class MediaPackage : BaseEntity
{
    protected MediaPackage() { }

    public string Identifier { get; set; } = string.Empty;

    public MediaPackage(string identifier)
    {
        Identifier = identifier;
    }

    public HealthState State { get; set; } = HealthState.Health;
    public ICollection<MediaAsset> Assets { get; set; } = new List<MediaAsset>();

    public MediaPackage(string identifier, string filename)
    {
        Id = Guid.NewGuid();
        Identifier = identifier;

        AddEvent(new ValidateMediaAsset(filename));
    }
}

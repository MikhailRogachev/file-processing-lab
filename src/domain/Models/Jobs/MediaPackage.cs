using domain.Events.FileEvents;

namespace domain.Models.Jobs;

public class MediaPackage : BaseEntity
{
    protected MediaPackage() { }

    public string Identifier { get; private set; } = string.Empty;
    public HealthState State { get; set; } = HealthState.Health;
    public ICollection<MediaAsset> Assets { get; set; } = new List<MediaAsset>();

    public MediaPackage(string identifier)
    {
        Id = Guid.NewGuid();
        Identifier = identifier;
    }

    public void ValidateAsset(string filename, string etag, long size)
    {
        var asset = Assets.FirstOrDefault(p => p.Filename == filename);
        if (asset == null)
        {
            Assets.Add(new MediaAsset(this.Id, filename)
            {
                Id = Guid.NewGuid(),
                Size = size,
                Etag = etag,
                Version = 1,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            asset.Size = size;
            asset.Etag = etag;
        }

        AddEvents(new ValidateMediaAssetEvent(Guid.NewGuid(), Id, DateTimeOffset.UtcNow, filename));
    }
}

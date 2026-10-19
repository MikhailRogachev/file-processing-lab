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

    #region Events

    public void AssetAdded(string fileName, string eTag, long size)
    {




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
                CurrentState = Contracts.Enums.State.Created,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            asset.Size = size;
            asset.Etag = etag;
            asset.Version = asset.Version + 1;
            asset.AssetType = AssetType.Unknown;
            asset.CurrentState = Contracts.Enums.State.Created;
            asset.Result = string.Empty;
            asset.Details = string.Empty;
            asset.UpdatedAt = DateTimeOffset.UtcNow;
        }

        AddEvents(new ValidateMediaAssetEvent(Guid.NewGuid(), Id, DateTimeOffset.UtcNow, filename));
    }

    #endregion
}

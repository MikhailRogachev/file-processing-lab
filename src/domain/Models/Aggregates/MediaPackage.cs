using domain.Dtos;

namespace domain.Models.Aggregates;

public class MediaPackage : BaseEntity
{
    protected MediaPackage() { }
    public string Identifier { get; private set; } = string.Empty;
    public ICollection<MediaAsset> Assets { get; set; } = new List<MediaAsset>();

    public MediaPackage(string identifier)
    {
        Id = Guid.NewGuid();
        Identifier = identifier;
    }

    #region Events

    public void CreateAsset(AssetCreateContext context)
    {
        UpdatedAt = DateTimeOffset.UtcNow;

        var asset = Assets.FirstOrDefault(p => p.Filename == context.FullFileName);

        if (asset == null)
        {
            Assets.Add(new MediaAsset(Id, context.FullFileName)
            {
                Id = Guid.NewGuid(),
                Size = context.Size,
                Etag = context.etag,
                Version = 1,
                Priority = context.Priority,
                CurrentStatus = AssetStatus.Created,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            asset.Size = context.Size;
            asset.Etag = context.etag;
            asset.Version = asset.Version + 1;
            asset.Priority = context.Priority;
            asset.AssetType = AssetType.Unknown;
            asset.CurrentStatus = AssetStatus.Created;
            asset.Result = string.Empty;
            asset.Details = string.Empty;
            asset.UpdatedAt = DateTimeOffset.UtcNow;
        }

        AddEvents(new ValidateMediaAssetEvent(Guid.NewGuid(), Id, DateTimeOffset.UtcNow, context.FullFileName));
    }

    #endregion
}

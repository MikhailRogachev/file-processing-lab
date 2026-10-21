using System.ComponentModel.DataAnnotations;

namespace domain.Models.Aggregates;

public class MediaAsset : BaseEntity
{
    public string Filename { get; set; } = string.Empty;

    public AssetType AssetType { get; set; }

    public AssetStatus CurrentStatus { get; set; }

    public long Size { get; set; }

    public string Etag { get; set; }

    public int Version { get; set; }

    [MaxLength(200)] public string Result { get; set; } = string.Empty;

    [MaxLength(800)] public string Details { get; set; } = string.Empty;

    public int Priority { get; set; }

    public Guid MediaPackageId { get; set; }

    public MediaPackage MediaPackage { get; set; }

    public MediaAsset(Guid mediaPackageId, string fileName)
    {
        Id = Guid.NewGuid();
        MediaPackageId = mediaPackageId;
        Filename = fileName;
    }

    protected MediaAsset() { }

    #region domains events

    public void Validated(AssetType assetType)
    {
        AssetType = assetType;
        CurrentStatus = AssetStatus.Validated;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ValidationFailed(string message, string details = "")
    {
        CurrentStatus = AssetStatus.ValidationFailed;
        UpdatedAt = DateTimeOffset.UtcNow;
        Result = message;
        Details = !string.IsNullOrWhiteSpace(details) ? details : message;
    }

    #endregion
}

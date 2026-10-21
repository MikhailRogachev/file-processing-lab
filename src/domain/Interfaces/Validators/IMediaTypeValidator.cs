using domain.Models.References;

namespace domain.Interfaces.Validators;

public interface IMediaTypeValidator
{
    Task<bool> ValidateMediaFileAsync(string filename, CancellationToken cancellationToken);

    Task<AllowedFileType?> GetAllowedAssetTypeAsync(string filename, CancellationToken cancellationToken);
}

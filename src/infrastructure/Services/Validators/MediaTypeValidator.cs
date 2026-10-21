using domain.Models.References;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.Services.Validators;

public class MediaTypeValidator(
    ILogger<MediaTypeValidator> logger,
    IServiceScopeFactory scopeFactory) : IMediaTypeValidator
{
    private readonly List<AllowedFileType> _mediaTypes = new List<AllowedFileType>();
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);


    public async Task<IReadOnlyCollection<AllowedFileType>> AllowedFileTypesAsync(CancellationToken cancellationToken)
    {
        if (_mediaTypes.Count > 0)
        {
            return _mediaTypes.AsReadOnly();
        }

        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_mediaTypes.Count == 0)
            {
                using var scope = scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IAllowedTypesRepository>();

                var list = await repo.GetCollectionAsync(cancellationToken);

                if (list.Any())
                {
                    _mediaTypes.AddRange(list);
                }
            }
        }
        finally
        {
            semaphore.Release();
        }

        return _mediaTypes.AsReadOnly();
    }

    public async Task<AllowedFileType?> GetAllowedAssetTypeAsync(string filename, CancellationToken cancellationToken)
    {
        var fileExtension = Path.GetExtension(filename)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            logger.LogWarning("Media file has no extension: {Filename}", filename);
            return default(AllowedFileType);
        }

        return await GetTypeAsync(fileExtension, cancellationToken);
    }

    public async Task<bool> ValidateMediaFileAsync(string filename, CancellationToken cancellationToken)
    {
        logger.LogDebug("Validating media file: {Filename}", filename);

        try
        {
            var fileExtension = Path.GetExtension(filename)?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                logger.LogWarning("Media file has no extension: {Filename}", filename);
                return false;
            }

            return await GetTypeAsync(fileExtension, cancellationToken) == null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while validating media file: {Filename}", filename);
            throw;
        }
    }

    private async Task<AllowedFileType?> GetTypeAsync(string extension, CancellationToken cancellationToken)
    {
        var items = await AllowedFileTypesAsync(cancellationToken);

        // Match extension whether DB stores with or without leading dot
        return items!.FirstOrDefault(p => p.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase) || p.Extension.Equals(extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
    }
}

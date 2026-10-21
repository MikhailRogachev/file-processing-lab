using domain.Dtos;
using domain.Models.References;
using Microsoft.Extensions.DependencyInjection;

namespace infrastructure.EventHandlers.FileUpdated;

public class FileValidateEventHandler(
    ILogger<FileValidateEventHandler> logger,
    IServiceScopeFactory scopeFactory
    ) : IEventHandler<ValidateMediaAssetEvent>
{

    public async Task HandleAsync(ValidateMediaAssetEvent @event, CancellationToken cancellationToken)
    {
        logger.LogDebug("Validating Asset {fn}", @event.FileName);
        var assetMediaType = await GetAllowedTypeAsync(@event.FileName, cancellationToken);


        try
        {
            using var scope = scopeFactory.CreateScope();
            var validator = GetValidator(scope, assetMediaType.Extension);
            var repository = scope.ServiceProvider.GetRequiredService<IMediaPackageRepository>();

            // validate asset
            var validationResult = await validator.ValidateAsync(new FileProbeContext
            (
                validator.BucketName,
                @event.FileName,
                assetMediaType.Extension,
                string.Empty
            ), cancellationToken);

            // get package and job
            var mediaPackage = await repository.GetAsync(@event.AggregateId, cancellationToken);
            var asset = mediaPackage!.Assets.FirstOrDefault(p => p.Filename == @event.FileName);

            // validation result processing
            if (validationResult)
            {
                asset!.Validated(assetMediaType.AssetType);
            }
            else
            {
                asset!.ValidationFailed($"The file {@event.FileName} has invalid format.");
            }

            await repository.UnitOfWork.SaveAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The error occured during validating file {fn}: {msg}", @event.FileName, ex.Message);
            throw;
        }
    }

    private async Task<AllowedFileType> GetAllowedTypeAsync(string filename, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMediaTypeValidator>();
        var item = await service.GetAllowedAssetTypeAsync(filename, cancellationToken);

        return item ?? throw new InvalidOperationException($"The file {filename} is not allowed for processing");
    }

    private static IFileProbeValidator GetValidator(IServiceScope scope, string extension)
    {
        var validator = scope.ServiceProvider.GetKeyedService<IFileProbeValidator>(extension);
        return validator ?? throw new NotSupportedException($"No file probe validator registered for extension '{extension}'.");
    }

}

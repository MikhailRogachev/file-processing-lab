using domain.Dtos;
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
        var extension = Path.GetExtension(@event.FileName);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var validator = GetValidator(scope, extension);
            var packageRepository = scope.ServiceProvider.GetRequiredService<IMediaPackageRepository>();
            var typesRepository = scope.ServiceProvider.GetRequiredService<IAllowedTypesRepository>();

            var mediatype = await typesRepository.GetAsync(extension, cancellationToken);
            if (mediatype == null)
            {
                throw new ArgumentException($"The extension is not recognized for the file {@event.FileName}");
            }

            // get package and job
            var mediaPackage = await packageRepository.GetAsync(@event.AggregateId, cancellationToken);
            var asset = mediaPackage!.Assets.FirstOrDefault(p => p.Filename == @event.FileName);

            // validate asset
            var validationResult = await validator.ValidateAsync(new FileProbeContext
            (
                validator.BucketName,
                @event.FileName,
                extension,
                string.Empty
            ), cancellationToken);

            // validation result processing
            if (validationResult)
            {
                asset!.Validated(mediatype.AssetType);
                await packageRepository.UnitOfWork.SaveAsync(cancellationToken);
            }
            else
            {

            }
        }
        catch (Exception ex)
        {
        }
    }

    private IFileProbeValidator GetValidator(IServiceScope scope, string extension) => extension switch
    {
        FileFormatKeys.MP4 => scope.ServiceProvider.GetRequiredKeyedService<IFileProbeValidator>(FileFormatKeys.MP4),
        FileFormatKeys.MXF => scope.ServiceProvider.GetRequiredKeyedService<IFileProbeValidator>(FileFormatKeys.MXF),
        FileFormatKeys.SRT => scope.ServiceProvider.GetRequiredKeyedService<IFileProbeValidator>(FileFormatKeys.SRT),
        FileFormatKeys.STL => scope.ServiceProvider.GetRequiredKeyedService<IFileProbeValidator>(FileFormatKeys.STL),
        _ => throw new NotSupportedException()
    };

}

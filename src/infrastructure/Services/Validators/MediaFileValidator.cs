namespace infrastructure.Services.Validators;

public class MediaFileValidator(
    ILogger<MediaFileValidator> logger,
    AppDbContext context) : IMediaFileValidator
{
    public async Task<bool> ValidateMediaFileAsync(string filename, CancellationToken cancellationToken)
    {
        logger.LogDebug("Validating media file: {Filename}", filename);

        try
        {
            var fileExtension = Path.GetExtension(filename)?.ToLowerInvariant();

            // if it's empty
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                logger.LogWarning("Media file has no extension: {Filename}", filename);
                return false;
            }

            var allowedExtension = await context.AllowedFileTypes.FirstOrDefaultAsync(p => p.Extension == fileExtension && p.IsActive);

            // Check if the media file type exists in the database
            //if (!await context.AllowedFileTypes.AnyAsync(p => p.Extension == fileExtension && p.IsActive, cancellationToken) == false)
            //{
            //    logger.LogWarning("Media file type not allowed: {FileExtension}", fileExtension);
            //    return false;
            //}

            //logger.LogInformation("Media file type is validated successfully: {Filename}", filename);
            return allowedExtension != null;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while validating media file: {Filename}", filename);
            throw;
        }
    }
}

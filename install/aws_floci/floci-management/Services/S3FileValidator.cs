using floci_management.Validators;

namespace floci_management.Services;

public class S3FileValidator
{
    public bool IsValid { get; private set; }

    public async Task<string> ValidateFileAsync(string filename, ConnectionTarget target, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(filename);
        if (string.IsNullOrEmpty(extension))
        {
            throw new ArgumentNullException(string.Format("The extension for the file {0} is nor found.", filename));
        }

        var service = GetValidator(extension, target);
        var response = await service.ValidateAsync(new FileProbe
        {
            S3Bucket = service.Bucket,
            S3Key = filename,
            Extension = extension
        }, cancellationToken);

        IsValid = service.IsValid;

        return response.Context!.ToString();
    }

    private S3BaseValidator GetValidator(string extension, ConnectionTarget target) => extension switch
    {
        ".mp4" => new S3Mp4Validator(target),
        ".mxf" => new S3MxfValidator(target),
        ".avi" => new S3AviValidator(target),
        ".srt" => new S3SrtValidator(target),
        ".stl" => new S3StlValidator(target),
        ".png" => new S3PngValidator(target),
        _ => throw new NotSupportedException()
    };
}

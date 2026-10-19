using domain.Dtos;

namespace domain.Interfaces.Validators;

public interface IFileProbeValidator
{
    string Name { get; }
    string BucketName { get; }
    string Folder { get; }
    Task<bool> ValidateAsync(FileProbeContext context, CancellationToken cancellationToken);
}

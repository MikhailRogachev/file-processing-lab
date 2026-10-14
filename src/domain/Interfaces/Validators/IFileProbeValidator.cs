using domain.Dtos;

namespace domain.Interfaces.Validators;

public interface IFileProbeValidator
{
    string Name { get; }
    Task<bool> ValidateAsync(FileProbeContext context, CancellationToken cancellationToken);
}

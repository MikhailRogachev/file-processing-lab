namespace domain.Interfaces.Validators;

public interface IMediaFileValidator
{
    Task<bool> ValidateMediaFileAsync(string filename, CancellationToken cancellationToken);
}

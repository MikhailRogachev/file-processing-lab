using domain.Models.References;

namespace domain.Interfaces.Repository;

public interface IAllowedTypesRepository
{
    Task<AllowedFileType?> GetAsync(string extension, CancellationToken cancellationToken);
    Task<IList<AllowedFileType>> GetCollectionAsync(CancellationToken cancellationToken);
}

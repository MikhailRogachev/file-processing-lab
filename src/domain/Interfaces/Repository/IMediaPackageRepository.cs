using domain.Interfaces.Data;
using domain.Models.Jobs;

namespace domain.Interfaces.Repository;

public interface IMediaPackageRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<MediaPackage?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<MediaPackage?> GetAsync(string identifier, CancellationToken cancellationToken);
    Task<MediaPackage> CreateAsync(string filename, CancellationToken cancellationToken);
    void Add(MediaPackage mediaPackage);
}

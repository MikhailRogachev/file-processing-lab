using domain.Dtos;
using domain.Interfaces.Data;

namespace domain.Interfaces.Repository;

public interface IMediaPackageRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<MediaPackage?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<MediaPackage?> GetAsync(string identifier, CancellationToken cancellationToken);
    Task<MediaPackage> CreateAsync(AssetCreateContext context, CancellationToken cancellationToken);
    void Add(MediaPackage mediaPackage);
}

using data.Context;
using domain.Dtos;
using domain.Extensions;
using domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace data.Repositories;

/// <summary>
/// Provides data access, aggregate root retrieval, and persistence management for <see cref="MediaPackage"/> domain entities.
/// </summary>
/// <remarks>
/// Encapsulates Entity Framework Core database context operations, isolates persistence logic from application workers,
/// and coordinates transactional operations through the <see cref="IUnitOfWork"/> interface.
/// </remarks>
public class MediaPackageRepository(
    ILogger<MediaPackageRepository> logger,
    AppDbContext context) : IMediaPackageRepository
{

    /// <summary>
    /// Gets the unit of work coordinator for atomic transaction control across repositories.
    /// </summary>
    public IUnitOfWork UnitOfWork => context;

    /// <summary>
    /// Registers a new <see cref="MediaPackage"/> aggregate root with the tracking context in the <see cref="EntityState.Added"/> state.
    /// </summary>
    /// <param name="mediaPackage">The <see cref="MediaPackage"/> instance to register.</param>
    /// <remarks>
    /// This is an in-memory operation. Database insertion occurs when <see cref="IUnitOfWork.SaveAsync(CancellationToken)"/> is called.
    /// </remarks>
    public void Add(MediaPackage mediaPackage)
    {
        try
        {
            context.MediaPackages.Add(mediaPackage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }

    /// <summary>
    /// Asynchronously retrieves an existing <see cref="MediaPackage"/> by file identifier or provisions a new instance if none exists.
    /// </summary>
    /// <param name="filename">The name or key path of the incoming media file.</param>
    /// <param name="cancellationToken">A token to observe while awaiting the database operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the tracked <see cref="MediaPackage"/> aggregate root.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filename"/> is null, empty, or consists only of whitespace.</exception>
    /// <remarks>
    /// Generates a unique business identifier from the filename, attaches or retrieves the aggregate root, 
    /// and invokes <see cref="MediaPackage.ValidateAsset(string)"/> to attach domain events.
    /// </remarks>
    public async Task<MediaPackage> CreateAsync(AssetCreateContext context, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(context.FileName))
            throw new ArgumentException("Filename cannot be null or empty.", nameof(context.FileName));

        var identifier = !string.IsNullOrWhiteSpace(context.identifier) ? context.identifier : context.FileName.CreateAssetIdentificator();
        var package = await GetAsync(identifier, cancellationToken);

        if (package == null)
        {
            package = new MediaPackage(identifier);
            Add(package);
        }

        package.CreateAsset(context);
        return package;
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="MediaPackage"/> by its string business identifier.
    /// </summary>
    /// <param name="identifier">The unique package business identifier.</param>
    /// <param name="cancellationToken">A token to observe while awaiting the database operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the matching <see cref="MediaPackage"/> 
    /// instance if found; otherwise, <see langword="null"/>.
    /// </returns
    public async Task<MediaPackage?> GetAsync(string identifier, CancellationToken cancellationToken)
    {
        return await context.MediaPackages
            .Include(x => x.Assets)
            .FirstOrDefaultAsync(x => x.Identifier == identifier, cancellationToken);
    }

    /// <summary>
    /// Asynchronously retrieves a <see cref="MediaPackage"/> by its primary key.
    /// </summary>
    /// <param name="id">The unique primary key <see cref="Guid"/> of the package.</param>
    /// <param name="cancellationToken">A token to observe while awaiting the database operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the matching <see cref="MediaPackage"/> 
    /// instance if found; otherwise, <see langword="null"/>.
    /// </returns>
    public async Task<MediaPackage?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.MediaPackages
            .Include(x => x.Assets)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}

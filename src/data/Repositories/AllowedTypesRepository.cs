using data.Context;
using domain.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace data.Repositories;

public class AllowedTypesRepository(AppDbContext context) : IAllowedTypesRepository
{
    public async Task<AllowedFileType?> GetAsync(string extension, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return default;
        }

        return await context.AllowedFileTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Extension.ToLower() == extension.ToLower());
    }

    public async Task<IList<AllowedFileType>> GetCollectionAsync(CancellationToken cancellationToken)
    {
        return await context.AllowedFileTypes.AsNoTracking().ToListAsync(cancellationToken);
    }
}

using data.Context;
using Microsoft.EntityFrameworkCore;

namespace api.Extensions;

public static class DatabaseMigration
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Handle migration exceptions if needed
            throw new InvalidOperationException("An error occurred while migrating the database.", ex);
        }
    }
}

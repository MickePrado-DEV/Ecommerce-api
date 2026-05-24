using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Persistence.Sql;

public static class DatabaseBootstrap
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<EcommerceDbContext>>();

        try
        {
            await db.Database.EnsureCreatedAsync(ct);
            await DbSeeder.SeedAsync(db, ct);
            logger.LogInformation("Base de datos inicializada correctamente ({Provider})", db.Database.ProviderName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,
                "No se pudo conectar a la base de datos. " +
                "SQL Server: crea la BD 'ecommerce', inicia el servicio y usa el perfil 'SqlServer'. " +
                "SQLite: usa el perfil 'Sqlite'. " +
                "Si el puerto está ocupado, detén otras instancias de la API o cambia applicationUrl en launchSettings.");
            throw;
        }
    }
}

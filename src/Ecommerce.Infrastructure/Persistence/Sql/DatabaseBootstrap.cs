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
            var cs = db.Database.GetConnectionString() ?? "(sin connection string)";
            var server = cs.Contains("localdb", StringComparison.OrdinalIgnoreCase) ? "LocalDB" : "SQL Server";
            logger.LogCritical(ex,
                "No se pudo conectar a {Server}. ConnectionString: {ConnectionString}. " +
                "LocalDB: perfil 'SqlServer' (por defecto). Instancia completa: perfil 'SqlServer (localhost)'. " +
                "Sin SQL: perfil 'Sqlite'.",
                server, MaskConnectionString(cs));
            throw;
        }
    }

    private static string MaskConnectionString(string cs) =>
        string.Join(';', cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !p.TrimStart().StartsWith("Password", StringComparison.OrdinalIgnoreCase)));
}

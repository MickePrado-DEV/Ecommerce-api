using Microsoft.Data.SqlClient;
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

            if (!await SchemaIsCurrentAsync(db, ct))
            {
                logger.LogWarning(
                    "La base de datos existe pero el esquema no coincide con el modelo actual. Recreando tablas...");
                await db.Database.EnsureDeletedAsync(ct);
                await db.Database.EnsureCreatedAsync(ct);
            }

            await EnsureAddressForeignKeysAsync(db, ct);
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

    private static async Task<bool> SchemaIsCurrentAsync(EcommerceDbContext db, CancellationToken ct)
    {
        try
        {
            await db.Users.AsNoTracking().Select(u => u.IsActive).Take(1).ToListAsync(ct);
            await db.Drivers.AsNoTracking().Select(d => d.UserId).Take(1).ToListAsync(ct);
            await db.Coupons.AsNoTracking().Select(c => c.Code).Take(1).ToListAsync(ct);
            // Tabla addresses ampliada (Type, colonia, lat/lng, etc.)
            await db.Addresses.AsNoTracking()
                .Select(a => new { a.Type, a.Neighborhood, a.Latitude, a.Longitude, a.IsDefault })
                .Take(1)
                .ToListAsync(ct);
            return true;
        }
        catch (Exception ex) when (IsSchemaMismatch(ex))
        {
            return false;
        }
    }

    private static bool IsSchemaMismatch(Exception ex) =>
        ex switch
        {
            SqlException { Number: 207 } => true,
            _ when ex.Message.Contains("no such column", StringComparison.OrdinalIgnoreCase) => true,
            _ when ex.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase) => true,
            _ when ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) => true,
            { InnerException: not null } => IsSchemaMismatch(ex.InnerException),
            _ => false
        };

    /// <summary>Quita FKs legacy en addresses que no referencian users (error 547 al insertar).</summary>
    private static async Task EnsureAddressForeignKeysAsync(EcommerceDbContext db, CancellationToken ct)
    {
        if (!db.Database.IsSqlServer()) return;

        await db.Database.ExecuteSqlRawAsync(
            """
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += N'ALTER TABLE addresses DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';'
            FROM sys.foreign_keys fk
            WHERE fk.parent_object_id = OBJECT_ID(N'dbo.addresses')
              AND fk.referenced_object_id IS NOT NULL
              AND fk.referenced_object_id <> OBJECT_ID(N'dbo.users');
            IF LEN(@sql) > 0 EXEC sp_executesql @sql;
            """, ct);
    }

    private static string MaskConnectionString(string cs) =>
        string.Join(';', cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !p.TrimStart().StartsWith("Password", StringComparison.OrdinalIgnoreCase)));
}

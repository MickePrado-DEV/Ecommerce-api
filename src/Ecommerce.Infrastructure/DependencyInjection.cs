using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Infrastructure.Documents;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Sql;
using Ecommerce.Infrastructure.Persistence.Sql.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var provider = Enum.Parse<DatabaseProvider>(config["Persistence:Provider"] ?? "Sqlite", true);
        var cs = config.GetConnectionString("Default")!;

        services.AddDbContextPool<EcommerceDbContext>(o =>
        {
            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    o.UseSqlServer(cs);
                    break;
                case DatabaseProvider.Sqlite:
                    o.UseSqlite(cs);
                    break;
                case DatabaseProvider.MySql:
                case DatabaseProvider.MariaDb:
                    throw new NotSupportedException(
                        "MySQL/MariaDB requiere Pomelo.EntityFrameworkCore.MySql 10.x (aún no publicado en NuGet). " +
                        "Usa SqlServer o Sqlite en Persistence:Provider.");
                default:
                    throw new InvalidOperationException($"Proveedor no soportado: {provider}");
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IAdminCatalogRepository, AdminCatalogRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPdfTicketGenerator, PdfTicketGenerator>();

        return services;
    }
}

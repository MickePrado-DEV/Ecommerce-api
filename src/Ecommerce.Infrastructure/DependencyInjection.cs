// Registro de implementaciones técnicas: base de datos, repositorios, JWT, PDF.
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
        // Proveedor de BD desde appsettings: SqlServer o Sqlite
        var provider = Enum.Parse<DatabaseProvider>(config["Persistence:Provider"] ?? "Sqlite", true);
        var cs = config.GetConnectionString("Default")!;

        // DbContext en pool: una instancia por petición HTTP (Scoped implícito con pool)
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

        // Transacciones explícitas (checkout, pago)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositorios de escritura y lectura (CQRS: *ReadRepository para proyecciones)
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICatalogReadRepository, CatalogReadRepository>();
        services.AddScoped<IAddressReadRepository, AddressReadRepository>();
        services.AddScoped<IAddressWriteRepository, AddressWriteRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminUserRepository, AdminUserRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IAdminCatalogRepository, AdminCatalogRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<ICoverRepository, CoverRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IProductOptionRepository, ProductOptionRepository>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();

        // Servicios de infraestructura usados por handlers
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPdfTicketGenerator, PdfTicketGenerator>();

        return services;
    }
}

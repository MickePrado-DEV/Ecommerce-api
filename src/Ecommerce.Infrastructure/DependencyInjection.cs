using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var provider = Enum.Parse<DatabaseProvider>(config["Persistence:Provider"] ?? "MySql", true);
            var cs = config.GetConnectionString("Default")!;

            services.AddDbContextPool<EcommerceDbContext>(o =>
            {
                if (provider is DatabaseProvider.SqlServer)
                    o.UseSqlServer(cs);
                else
                    o.UseMySql(cs, ServerVersion.AutoDetect(cs));
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}

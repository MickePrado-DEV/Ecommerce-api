using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Validators;
using Microsoft.Extensions.DependencyInjection;
namespace Ecommerce.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICheckoutService, CheckoutService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IInventoryService, InventoryService>();
            return services;
        }
    }
}

using Ecommerce.Application.Abstractions;

namespace Ecommerce.Application.Services
{
    public class CatalogService : ICatalogService
    {
        public Task<object> GetFamiliesAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<object?> GetProductBySlugAsync(string slug, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<object> ListProductsAsync(int page, int pageSize, string? search, CancellationToken ct = default) => throw new NotImplementedException();
    }
}

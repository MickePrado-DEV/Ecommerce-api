using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence
{
    public interface IProductRepository
    {
        Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
        Task<(List<Product> Items, int Total)> ListAsync(CatalogProductQuery query, CancellationToken ct = default);
        Task<List<Product>> ListLatestAsync(int take, CancellationToken ct = default);
    }
}

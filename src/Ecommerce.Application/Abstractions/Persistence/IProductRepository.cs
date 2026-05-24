using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence
{
    public interface IProductRepository
    {
        Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
        Task<(List<Product> Items, int Total)> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    }
}

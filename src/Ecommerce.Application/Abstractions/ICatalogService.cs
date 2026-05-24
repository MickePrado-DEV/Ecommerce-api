namespace Ecommerce.Application.Abstractions
{
    public interface ICatalogService
    {
        Task<object> GetFamiliesAsync(CancellationToken ct = default);
        Task<object?> GetProductBySlugAsync(string slug, CancellationToken ct = default);
        Task<object> ListProductsAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    }
}

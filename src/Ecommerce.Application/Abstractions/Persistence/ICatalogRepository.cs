using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface ICatalogRepository
{
    Task<List<Family>> GetFamiliesTreeAsync(CancellationToken ct = default);
    Task<Family?> GetFamilyBySlugAsync(string slug, CancellationToken ct = default);
    Task<Category?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default);
    Task<Subcategory?> GetSubcategoryBySlugAsync(string slug, CancellationToken ct = default);
}

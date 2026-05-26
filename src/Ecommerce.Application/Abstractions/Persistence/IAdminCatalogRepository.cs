using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IAdminCatalogRepository
{
    Task<Family?> GetFamilyAsync(Guid id, CancellationToken ct = default);
    Task<List<Family>> ListFamiliesAsync(CancellationToken ct = default);
    Task<(List<Family> Items, int Total)> ListFamiliesPagedAsync(AdminTableQueryParams query, CancellationToken ct = default);
    Task<(List<CategoryAdminRowDto> Items, int Total)> ListCategoriesPagedAsync(AdminTableQueryParams query, CancellationToken ct = default);
    Task<(List<SubcategoryAdminRowDto> Items, int Total)> ListSubcategoriesPagedAsync(AdminTableQueryParams query, CancellationToken ct = default);
    Task<Family> SaveFamilyAsync(Family entity, CancellationToken ct = default);
    Task DeleteFamilyAsync(Guid id, CancellationToken ct = default);

    Task<Category?> GetCategoryAsync(Guid id, CancellationToken ct = default);
    Task<Category> SaveCategoryAsync(Category entity, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    Task<Subcategory?> GetSubcategoryAsync(Guid id, CancellationToken ct = default);
    Task<Subcategory> SaveSubcategoryAsync(Subcategory entity, CancellationToken ct = default);
    Task DeleteSubcategoryAsync(Guid id, CancellationToken ct = default);

    Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default);
    Task<(List<Product> Items, int Total)> ListProductsAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Product> SaveProductAsync(Product entity, CancellationToken ct = default);
    Task DeleteProductAsync(Guid id, CancellationToken ct = default);

    Task<Variant?> GetVariantAsync(Guid id, CancellationToken ct = default);
    Task<Variant> SaveVariantAsync(Variant entity, CancellationToken ct = default);
    Task DeleteVariantAsync(Guid id, CancellationToken ct = default);
}

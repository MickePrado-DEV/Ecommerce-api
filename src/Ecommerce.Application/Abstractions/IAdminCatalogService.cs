using Ecommerce.Application.DTOs.Admin;

namespace Ecommerce.Application.Abstractions;

public interface IAdminCatalogService
{
    Task<IReadOnlyList<FamilyAdminDto>> ListFamiliesAsync(CancellationToken ct = default);
    Task<FamilyAdminDto> SaveFamilyAsync(SaveFamilyRequest request, CancellationToken ct = default);
    Task DeleteFamilyAsync(Guid id, CancellationToken ct = default);
    Task<CategoryAdminDto> SaveCategoryAsync(SaveCategoryRequest request, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);
    Task<SubcategoryAdminDto> SaveSubcategoryAsync(SaveSubcategoryRequest request, CancellationToken ct = default);
    Task DeleteSubcategoryAsync(Guid id, CancellationToken ct = default);
    Task<ProductAdminDto> SaveProductAsync(SaveProductRequest request, CancellationToken ct = default);
    Task DeleteProductAsync(Guid id, CancellationToken ct = default);
    Task<VariantAdminDto> SaveVariantAsync(SaveVariantRequest request, CancellationToken ct = default);
    Task DeleteVariantAsync(Guid id, CancellationToken ct = default);
    Task<PagedProductsAdminDto> ListProductsAsync(int page, int pageSize, CancellationToken ct = default);
}

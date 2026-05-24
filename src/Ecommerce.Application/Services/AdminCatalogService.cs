using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Services;

public class AdminCatalogService(IAdminCatalogRepository repo, IInventoryRepository inventory) : IAdminCatalogService
{
    public async Task<IReadOnlyList<FamilyAdminDto>> ListFamiliesAsync(CancellationToken ct = default)
    {
        var items = await repo.ListFamiliesAsync(ct);
        return items.Select(f => new FamilyAdminDto(f.Id, f.Name, f.Slug, f.SortOrder, f.IsActive)).ToList();
    }

    public async Task<FamilyAdminDto> SaveFamilyAsync(SaveFamilyRequest request, CancellationToken ct = default)
    {
        var entity = new Family
        {
            Id = request.Id ?? Guid.Empty,
            Name = request.Name,
            Slug = request.Slug,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };
        var saved = await repo.SaveFamilyAsync(entity, ct);
        return new FamilyAdminDto(saved.Id, saved.Name, saved.Slug, saved.SortOrder, saved.IsActive);
    }

    public Task DeleteFamilyAsync(Guid id, CancellationToken ct = default) => repo.DeleteFamilyAsync(id, ct);

    public async Task<CategoryAdminDto> SaveCategoryAsync(SaveCategoryRequest request, CancellationToken ct = default)
    {
        var entity = new Category
        {
            Id = request.Id ?? Guid.Empty,
            FamilyId = request.FamilyId,
            Name = request.Name,
            Slug = request.Slug,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };
        var saved = await repo.SaveCategoryAsync(entity, ct);
        return new CategoryAdminDto(saved.Id, saved.FamilyId, saved.Name, saved.Slug, saved.SortOrder, saved.IsActive);
    }

    public Task DeleteCategoryAsync(Guid id, CancellationToken ct = default) => repo.DeleteCategoryAsync(id, ct);

    public async Task<SubcategoryAdminDto> SaveSubcategoryAsync(SaveSubcategoryRequest request, CancellationToken ct = default)
    {
        var entity = new Subcategory
        {
            Id = request.Id ?? Guid.Empty,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Slug = request.Slug,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };
        var saved = await repo.SaveSubcategoryAsync(entity, ct);
        return new SubcategoryAdminDto(saved.Id, saved.CategoryId, saved.Name, saved.Slug, saved.SortOrder, saved.IsActive);
    }

    public Task DeleteSubcategoryAsync(Guid id, CancellationToken ct = default) => repo.DeleteSubcategoryAsync(id, ct);

    public async Task<ProductAdminDto> SaveProductAsync(SaveProductRequest request, CancellationToken ct = default)
    {
        var entity = new Product
        {
            Id = request.Id ?? Guid.Empty,
            SubcategoryId = request.SubcategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            BasePrice = request.BasePrice,
            IsActive = request.IsActive
        };
        var saved = await repo.SaveProductAsync(entity, ct);
        return new ProductAdminDto(saved.Id, saved.SubcategoryId, saved.Name, saved.Slug, saved.Description, saved.BasePrice, saved.IsActive);
    }

    public Task DeleteProductAsync(Guid id, CancellationToken ct = default) => repo.DeleteProductAsync(id, ct);

    public async Task<VariantAdminDto> SaveVariantAsync(SaveVariantRequest request, CancellationToken ct = default)
    {
        var entity = new Variant
        {
            Id = request.Id ?? Guid.Empty,
            ProductId = request.ProductId,
            Sku = request.Sku,
            Price = request.Price,
            IsActive = request.IsActive
        };
        var saved = await repo.SaveVariantAsync(entity, ct);
        if (request.InitialStock.HasValue)
            await inventory.UpsertAsync(saved.Id, request.InitialStock.Value, ct);
        var inv = await inventory.GetByVariantIdAsync(saved.Id, ct);
        return new VariantAdminDto(saved.Id, saved.ProductId, saved.Sku, saved.Price, saved.IsActive, inv?.QuantityOnHand ?? 0);
    }

    public Task DeleteVariantAsync(Guid id, CancellationToken ct = default) => repo.DeleteVariantAsync(id, ct);

    public async Task<PagedProductsAdminDto> ListProductsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var result = await repo.ListProductsAsync(page, pageSize, ct);
        return new PagedProductsAdminDto(
            result.Items.Select(p => new ProductAdminDto(p.Id, p.SubcategoryId, p.Name, p.Slug, p.Description, p.BasePrice, p.IsActive)).ToList(),
            result.Total, page, pageSize);
    }
}

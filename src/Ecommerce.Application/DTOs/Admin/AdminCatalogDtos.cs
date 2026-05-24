namespace Ecommerce.Application.DTOs.Admin;

public record FamilyAdminDto(Guid Id, string Name, string Slug, int SortOrder, bool IsActive);
public record SaveFamilyRequest(Guid? Id, string Name, string Slug, int SortOrder, bool IsActive);
public record CategoryAdminDto(Guid Id, Guid FamilyId, string Name, string Slug, int SortOrder, bool IsActive);
public record SaveCategoryRequest(Guid? Id, Guid FamilyId, string Name, string Slug, int SortOrder, bool IsActive);
public record SubcategoryAdminDto(Guid Id, Guid CategoryId, string Name, string Slug, int SortOrder, bool IsActive);
public record SaveSubcategoryRequest(Guid? Id, Guid CategoryId, string Name, string Slug, int SortOrder, bool IsActive);
public record ProductAdminDto(Guid Id, Guid SubcategoryId, string Name, string Slug, string? Description, decimal BasePrice, bool IsActive);
public record SaveProductRequest(Guid? Id, Guid SubcategoryId, string Name, string Slug, string? Description, decimal BasePrice, bool IsActive);
public record VariantAdminDto(Guid Id, Guid ProductId, string Sku, decimal? Price, bool IsActive, int QuantityOnHand);
public record SaveVariantRequest(Guid? Id, Guid ProductId, string Sku, decimal? Price, bool IsActive, int? InitialStock);
public record PagedProductsAdminDto(IReadOnlyList<ProductAdminDto> Items, int Total, int Page, int PageSize);

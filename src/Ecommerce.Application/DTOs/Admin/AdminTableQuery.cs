namespace Ecommerce.Application.DTOs.Admin;

public record AdminTableQueryParams(
    int Page = 1,
    int PageSize = 10,
    string? SortKey = null,
    string SortDir = "asc",
    string? Search = null,
    string? FamilyName = null,
    string? CategoryName = null);

public record PagedFamiliesAdminDto(
    IReadOnlyList<FamilyAdminDto> Items,
    int Total,
    int Page,
    int PageSize);

public record CategoryAdminRowDto(
    Guid Id,
    Guid FamilyId,
    string Name,
    string Slug,
    int SortOrder,
    bool IsActive,
    string FamilyName);

public record PagedCategoriesAdminDto(
    IReadOnlyList<CategoryAdminRowDto> Items,
    int Total,
    int Page,
    int PageSize);

public record SubcategoryAdminRowDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    int SortOrder,
    bool IsActive,
    string CategoryName,
    string FamilyName);

public record PagedSubcategoriesAdminDto(
    IReadOnlyList<SubcategoryAdminRowDto> Items,
    int Total,
    int Page,
    int PageSize);

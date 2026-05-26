namespace Ecommerce.Application.DTOs.Admin;

public record CoverAdminDto(
    Guid Id,
    string Title,
    string ImageUrl,
    string? LinkUrl,
    int SortOrder,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsEffectivelyActive);

public record PagedCoversAdminDto(
    IReadOnlyList<CoverAdminDto> Items,
    int Total,
    int Page,
    int PageSize);

public record SaveCoverRequest(
    Guid? Id,
    string Title,
    string ImageUrl,
    string? LinkUrl,
    bool IsActive,
    DateTime? StartsAt,
    DateTime? EndsAt);

public record ReorderCoversRequest(IReadOnlyList<Guid> Ids);

public record CoverImageUploadDto(string Url);

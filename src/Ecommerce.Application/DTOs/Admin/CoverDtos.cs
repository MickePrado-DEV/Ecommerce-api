namespace Ecommerce.Application.DTOs.Admin;

public record CoverAdminDto(
    Guid Id,
    string Title,
    string ImageUrl,
    string? LinkUrl,
    int SortOrder,
    bool IsActive);

public record SaveCoverRequest(
    Guid? Id,
    string Title,
    string ImageUrl,
    string? LinkUrl,
    int SortOrder,
    bool IsActive);

public record ReorderCoversRequest(IReadOnlyList<Guid> Ids);

namespace Ecommerce.Application.DTOs.Admin;

public record UserAdminDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    bool IsActive,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt);

public record PagedUsersAdminDto(
    IReadOnlyList<UserAdminDto> Items,
    int Total,
    int Page,
    int PageSize);

public record UpdateUserAdminRequest(bool? IsActive, IReadOnlyList<string>? RoleCodes);

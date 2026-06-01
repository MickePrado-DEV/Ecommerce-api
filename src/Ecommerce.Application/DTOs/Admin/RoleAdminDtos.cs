namespace Ecommerce.Application.DTOs.Admin;

public record RoleAdminDto(
    Guid Id,
    string Name,
    string Code,
    IReadOnlyList<string> PermissionCodes);

public record PermissionAdminDto(Guid Id, string Code, string Name);

public record UpdateRolePermissionsRequest(IReadOnlyList<string> PermissionCodes);

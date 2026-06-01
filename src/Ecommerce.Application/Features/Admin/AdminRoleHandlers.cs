using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Authorization;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Authorization;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin;

public record ListRolesAdminQuery : IRequest<Result<IReadOnlyList<RoleAdminDto>>>;

public class ListRolesAdminQueryHandler(IAdminRoleRepository repo)
    : IRequestHandler<ListRolesAdminQuery, Result<IReadOnlyList<RoleAdminDto>>>
{
    public async Task<Result<IReadOnlyList<RoleAdminDto>>> Handle(ListRolesAdminQuery request, CancellationToken ct)
    {
        var roles = await repo.ListAsync(ct);
        return Result.Ok((IReadOnlyList<RoleAdminDto>)roles.Select(MapRole).ToList());
    }

    internal static RoleAdminDto MapRole(Domain.Entities.Role role) => new(
        role.Id,
        role.Name,
        role.Code,
        role.RolePermissions.Select(rp => rp.Permission.Code).OrderBy(c => c).ToList());
}

public record GetRoleAdminQuery(Guid Id) : IRequest<Result<RoleAdminDto>>;

public class GetRoleAdminQueryHandler(IAdminRoleRepository repo)
    : IRequestHandler<GetRoleAdminQuery, Result<RoleAdminDto>>
{
    public async Task<Result<RoleAdminDto>> Handle(GetRoleAdminQuery request, CancellationToken ct)
    {
        var role = await repo.GetByIdAsync(request.Id, ct);
        return role is null
            ? Result.Fail<RoleAdminDto>(AdminErrors.NotFound("Role", request.Id))
            : Result.Ok(ListRolesAdminQueryHandler.MapRole(role));
    }
}

public record ListPermissionsAdminQuery : IRequest<Result<IReadOnlyList<PermissionAdminDto>>>;

public class ListPermissionsAdminQueryHandler(IAdminRoleRepository repo)
    : IRequestHandler<ListPermissionsAdminQuery, Result<IReadOnlyList<PermissionAdminDto>>>
{
    public async Task<Result<IReadOnlyList<PermissionAdminDto>>> Handle(ListPermissionsAdminQuery request, CancellationToken ct)
    {
        var permissions = await repo.ListPermissionsAsync(ct);
        var dtos = permissions.Select(p => new PermissionAdminDto(p.Id, p.Code, p.Name)).ToList();
        return Result.Ok((IReadOnlyList<PermissionAdminDto>)dtos);
    }
}

public record UpdateRolePermissionsCommand(Guid RoleId, IReadOnlyList<string> PermissionCodes)
    : IRequest<Result<RoleAdminDto>>;

public class UpdateRolePermissionsCommandHandler(IAdminRoleRepository repo)
    : IRequestHandler<UpdateRolePermissionsCommand, Result<RoleAdminDto>>
{
    public async Task<Result<RoleAdminDto>> Handle(UpdateRolePermissionsCommand request, CancellationToken ct)
    {
        var role = await repo.GetByIdAsync(request.RoleId, ct);
        if (role is null)
            return Result.Fail<RoleAdminDto>(AdminErrors.NotFound("Role", request.RoleId));

        if (string.Equals(role.Code, RoleCodes.Admin, StringComparison.Ordinal)
            && !request.PermissionCodes.Contains(AdminPermissions.UsersManage))
        {
            return Result.Fail<RoleAdminDto>(AdminErrors.Validation(
                "El rol administrador debe conservar al menos el permiso admin.users.manage."));
        }

        try
        {
            await repo.UpdatePermissionsAsync(request.RoleId, request.PermissionCodes, ct);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<RoleAdminDto>(AdminErrors.Validation(ex.Message));
        }

        var updated = await repo.GetByIdAsync(request.RoleId, ct);
        return updated is null
            ? Result.Fail<RoleAdminDto>(AdminErrors.NotFound("Role", request.RoleId))
            : Result.Ok(ListRolesAdminQueryHandler.MapRole(updated));
    }
}

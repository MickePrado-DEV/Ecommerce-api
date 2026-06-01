using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin;

public record CreateUserAdminCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    bool IsActive,
    IReadOnlyList<string> RoleCodes) : IRequest<Result<UserAdminDto>>;

public class CreateUserAdminCommandHandler(IAdminUserRepository repo, IUserRepository users)
    : IRequestHandler<CreateUserAdminCommand, Result<UserAdminDto>>
{
    public async Task<Result<UserAdminDto>> Handle(CreateUserAdminCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.EmailExistsAsync(email, ct))
            return Result.Fail<UserAdminDto>(AdminErrors.Conflict("El email ya está registrado."));

        var roleValidation = await AdminUserRoleValidation.ValidateRoleCodesAsync(request.RoleCodes, ct);
        if (roleValidation.IsFailed)
            return Result.Fail<UserAdminDto>(roleValidation.Errors);

        var user = await repo.CreateAsync(new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            IsActive = request.IsActive,
        }, request.RoleCodes, ct);

        return Result.Ok(AdminUserMapping.MapUser(user));
    }
}

public record UpdateUserAdminCommand(
    Guid Id,
    Guid? ActorUserId,
    bool? IsActive,
    IReadOnlyList<string>? RoleCodes) : IRequest<Result<UserAdminDto>>;

public class UpdateUserAdminCommandHandler(IAdminUserRepository repo)
    : IRequestHandler<UpdateUserAdminCommand, Result<UserAdminDto>>
{
    public async Task<Result<UserAdminDto>> Handle(UpdateUserAdminCommand request, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(request.Id, ct);
        if (user is null)
            return Result.Fail<UserAdminDto>(AdminErrors.NotFound("User", request.Id));

        if (request.RoleCodes is not null)
        {
            var roleValidation = await AdminUserRoleValidation.ValidateRoleCodesAsync(request.RoleCodes, ct);
            if (roleValidation.IsFailed)
                return Result.Fail<UserAdminDto>(roleValidation.Errors);

            if (request.ActorUserId == request.Id
                && !request.RoleCodes.Contains(RoleCodes.Admin, StringComparer.Ordinal)
                && user.Roles.Contains(RoleCodes.Admin, StringComparer.Ordinal))
            {
                return Result.Fail<UserAdminDto>(AdminErrors.Validation("No puedes quitarte tu propio rol de administrador."));
            }
        }

        if (request.IsActive == false && request.ActorUserId == request.Id)
            return Result.Fail<UserAdminDto>(AdminErrors.Validation("No puedes desactivar tu propia cuenta."));

        try
        {
            await repo.UpdateAsync(request.Id, request.IsActive, request.RoleCodes, ct);
        }
        catch (InvalidOperationException)
        {
            return Result.Fail<UserAdminDto>(AdminErrors.NotFound("User", request.Id));
        }

        var updated = await repo.GetByIdAsync(request.Id, ct);
        return updated is null
            ? Result.Fail<UserAdminDto>(AdminErrors.NotFound("User", request.Id))
            : Result.Ok(AdminUserMapping.MapUser(updated));
    }
}

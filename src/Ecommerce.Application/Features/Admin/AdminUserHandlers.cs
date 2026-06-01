using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin;

public record ListUsersAdminQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<Result<PagedUsersAdminDto>>;

public class ListUsersAdminQueryHandler(IAdminUserRepository repo)
    : IRequestHandler<ListUsersAdminQuery, Result<PagedUsersAdminDto>>
{
    public async Task<Result<PagedUsersAdminDto>> Handle(ListUsersAdminQuery request, CancellationToken ct)
    {
        var (items, total) = await repo.ListAsync(request.Page, request.PageSize, request.Search, ct);
        var dtos = items.Select(MapUser).ToList();
        return Result.Ok(new PagedUsersAdminDto(dtos, total, request.Page, request.PageSize));
    }

    internal static UserAdminDto MapUser(User u) => new(
        u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.IsActive, u.Roles, u.CreatedAt);
}

public record GetUserAdminQuery(Guid Id) : IRequest<Result<UserAdminDto>>;

public class GetUserAdminQueryHandler(IAdminUserRepository repo)
    : IRequestHandler<GetUserAdminQuery, Result<UserAdminDto>>
{
    public async Task<Result<UserAdminDto>> Handle(GetUserAdminQuery request, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(request.Id, ct);
        return user is null
            ? Result.Fail<UserAdminDto>(AdminErrors.NotFound("User", request.Id))
            : Result.Ok(ListUsersAdminQueryHandler.MapUser(user));
    }
}

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

        var roleValidation = await ValidateRoleCodesAsync(request.RoleCodes, ct);
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

        return Result.Ok(ListUsersAdminQueryHandler.MapUser(user));
    }

    internal static async Task<Result> ValidateRoleCodesAsync(IReadOnlyList<string> roleCodes, CancellationToken ct)
    {
        if (roleCodes.Count == 0)
            return Result.Fail(AdminErrors.Validation("Asigna al menos un rol."));

        var validCodes = new HashSet<string>(RoleCodes.All, StringComparer.Ordinal);
        var invalid = roleCodes.Where(c => !validCodes.Contains(c)).Distinct().ToList();
        if (invalid.Count > 0)
            return Result.Fail(AdminErrors.Validation($"Roles inválidos: {string.Join(", ", invalid)}"));

        return Result.Ok();
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
            var roleValidation = await CreateUserAdminCommandHandler.ValidateRoleCodesAsync(request.RoleCodes, ct);
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
            : Result.Ok(ListUsersAdminQueryHandler.MapUser(updated));
    }
}

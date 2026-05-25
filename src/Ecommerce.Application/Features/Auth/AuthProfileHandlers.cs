// Perfil de usuario: actualizar datos y cambiar contraseña.
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Domain.Auth;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Auth;

public record UpdateProfileCommand(Guid UserId, string FirstName, string LastName, string? Phone)
    : IRequest<Result<UserDto>>;

public class UpdateProfileCommandHandler(IUserRepository users, IDriverRepository drivers)
    : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdWithRolesAsync(request.UserId, ct);
        if (user is null)
            return Result.Fail<UserDto>(AuthErrors.UserNotFound());

        await users.UpdateProfileAsync(request.UserId, request.FirstName, request.LastName, request.Phone, ct);
        user = (await users.GetByIdWithRolesAsync(request.UserId, ct))!;
        var driver = await drivers.GetByUserIdAsync(user.Id, ct);
        return Result.Ok(await LoginCommandHandler.ToUserDtoAsync(user, driver));
    }
}

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<Result>;

public class ChangePasswordCommandHandler(IUserRepository users) : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdWithRolesAsync(request.UserId, ct);
        if (user is null)
            return Result.Fail(AuthErrors.UserNotFound());

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Fail(AuthErrors.InvalidCurrentPassword());

        var hash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await users.UpdatePasswordHashAsync(request.UserId, hash, ct);
        await users.RevokeRefreshTokensAsync(request.UserId, ct);
        return Result.Ok();
    }
}

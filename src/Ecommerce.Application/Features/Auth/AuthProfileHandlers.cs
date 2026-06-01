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
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Fail(AuthErrors.UserNotFound());

        var currentOk = user.MustChangePassword
            && !string.IsNullOrEmpty(user.TemporaryPasswordPlain)
            && string.Equals(request.CurrentPassword, user.TemporaryPasswordPlain, StringComparison.Ordinal)
            || BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!currentOk)
            return Result.Fail(AuthErrors.InvalidCurrentPassword());

        var hash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        if (user.MustChangePassword)
            await users.CompleteMandatoryPasswordChangeAsync(request.UserId, hash, ct);
        else
            await users.UpdatePasswordHashAsync(request.UserId, hash, ct);

        await users.RevokeRefreshTokensAsync(request.UserId, ct);
        return Result.Ok();
    }
}

public record MandatoryChangePasswordCommand(Guid UserId, string NewPassword) : IRequest<Result>;

public class MandatoryChangePasswordCommandHandler(IUserRepository users)
    : IRequestHandler<MandatoryChangePasswordCommand, Result>
{
    public async Task<Result> Handle(MandatoryChangePasswordCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Fail(AuthErrors.UserNotFound());

        if (!user.MustChangePassword)
            return Result.Fail(AuthErrors.InvalidState("No se requiere cambio de contraseña."));

        var hash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await users.CompleteMandatoryPasswordChangeAsync(request.UserId, hash, ct);
        await users.RevokeRefreshTokensAsync(request.UserId, ct);
        return Result.Ok();
    }
}

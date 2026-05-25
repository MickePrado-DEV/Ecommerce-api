// Handlers de autenticación (CQRS): login, registro, refresh, logout y perfil.
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Domain.Auth;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Auth;

/// <summary>Command: validar email/password y emitir tokens.</summary>
public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler(IUserRepository users, IJwtTokenService jwt)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await users.GetByEmailWithRolesAsync(request.Email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Fail<LoginResponse>(AuthErrors.InvalidCredentials());

        return Result.Ok(await BuildLoginResponseAsync(user, users, jwt, ct));
    }

    /// <summary>Genera JWT, refresh token y persiste hash del refresh en BD.</summary>
    internal static async Task<LoginResponse> BuildLoginResponseAsync(
        User user, IUserRepository users, IJwtTokenService jwt, CancellationToken ct)
    {
        var permissions = await users.GetPermissionsAsync(user.Id, ct);
        var access = jwt.GenerateAccessToken(user, permissions);
        var refresh = jwt.GenerateRefreshToken();
        await users.SaveRefreshTokenAsync(user.Id, refresh.Hash, refresh.ExpiresAt, ct);
        return new LoginResponse(access, refresh.Token, ToUserDto(user), permissions);
    }

    internal static UserDto ToUserDto(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.Roles);
}

/// <summary>Command: alta de usuario; devuelve tokens como en login.</summary>
public record RegisterCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<Result<LoginResponse>>;

public class RegisterCommandHandler(IUserRepository users, IJwtTokenService jwt)
    : IRequestHandler<RegisterCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await users.EmailExistsAsync(request.Email, ct))
            return Result.Fail<LoginResponse>(AuthErrors.EmailAlreadyRegistered());

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        await users.CreateAsync(user, ct);
        user = (await users.GetByEmailWithRolesAsync(request.Email, ct))!;
        return Result.Ok(await LoginCommandHandler.BuildLoginResponseAsync(user, users, jwt, ct));
    }
}

/// <summary>Command: intercambia refresh token por par nuevo (revoca los anteriores).</summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;

public class RefreshTokenCommandHandler(IUserRepository users, IJwtTokenService jwt)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = AuthTokenHasher.Hash(request.RefreshToken);
        var stored = await users.GetValidRefreshTokenAsync(hash, ct);
        if (stored is null)
            return Result.Fail<LoginResponse>(AuthErrors.InvalidCredentials());

        await users.RevokeRefreshTokensAsync(stored.UserId, ct);
        return Result.Ok(await LoginCommandHandler.BuildLoginResponseAsync(stored.User, users, jwt, ct));
    }
}

/// <summary>Command: invalida refresh tokens del usuario (logout).</summary>
public record LogoutCommand(Guid UserId) : IRequest<Result>;

public class LogoutCommandHandler(IUserRepository users) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        await users.RevokeRefreshTokensAsync(request.UserId, ct);
        return Result.Ok();
    }
}

/// <summary>Query: perfil del usuario autenticado.</summary>
public record GetMeQuery(Guid UserId) : IRequest<Result<UserDto>>;

public class GetMeQueryHandler(IUserRepository users) : IRequestHandler<GetMeQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdWithRolesAsync(request.UserId, ct);
        return user is null
            ? Result.Fail<UserDto>(AuthErrors.UserNotFound())
            : Result.Ok(LoginCommandHandler.ToUserDto(user));
    }
}

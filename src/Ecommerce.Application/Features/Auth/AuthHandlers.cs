// Handlers de autenticación: registro cliente/repartidor, login, refresh, logout, perfil.
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Domain.Auth;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Auth;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler(IUserRepository users, IJwtTokenService jwt, IDriverRepository drivers)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await users.GetByEmailWithRolesAsync(request.Email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Fail<LoginResponse>(AuthErrors.InvalidCredentials());

        return Result.Ok(await BuildLoginResponseAsync(user, users, jwt, drivers, ct));
    }

    internal static async Task<LoginResponse> BuildLoginResponseAsync(
        User user, IUserRepository users, IJwtTokenService jwt, IDriverRepository drivers, CancellationToken ct)
    {
        var permissions = await users.GetPermissionsAsync(user.Id, ct);
        var access = jwt.GenerateAccessToken(user, permissions);
        var refresh = jwt.GenerateRefreshToken();
        await users.SaveRefreshTokenAsync(user.Id, refresh.Hash, refresh.ExpiresAt, ct);
        var driver = await drivers.GetByUserIdAsync(user.Id, ct);
        return new LoginResponse(access, refresh.Token, await ToUserDtoAsync(user, driver), permissions);
    }

    internal static Task<UserDto> ToUserDtoAsync(User user, Domain.Entities.Driver? driver) =>
        Task.FromResult(new UserDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Roles,
            driver?.Id, user.Phone));
}

public record RegisterCustomerCommand(
    string Email, string Password, string FirstName, string LastName, string? Phone)
    : IRequest<Result<LoginResponse>>;

public class RegisterCustomerCommandHandler(IUserRepository users, IJwtTokenService jwt, IDriverRepository drivers)
    : IRequestHandler<RegisterCustomerCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RegisterCustomerCommand request, CancellationToken ct)
    {
        if (await users.EmailExistsAsync(request.Email, ct))
            return Result.Fail<LoginResponse>(AuthErrors.EmailAlreadyRegistered());

        var roleId = await users.GetRoleIdByCodeAsync(RoleCodes.Customer, ct);
        if (roleId is null)
            return Result.Fail<LoginResponse>(AuthErrors.RoleNotConfigured(RoleCodes.Customer));

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone
        };
        await users.CreateAsync(user, ct);
        await users.AssignRoleAsync(user.Id, roleId.Value, ct);

        user = (await users.GetByEmailWithRolesAsync(request.Email, ct))!;
        return Result.Ok(await LoginCommandHandler.BuildLoginResponseAsync(user, users, jwt, drivers, ct));
    }
}

public record RegisterDriverCommand(
    string Email, string Password, string FirstName, string LastName, string Phone,
    string? LicenseNumber, string? VehiclePlate)
    : IRequest<Result<LoginResponse>>;

public class RegisterDriverCommandHandler(
    IUserRepository users, IJwtTokenService jwt, IDriverRepository drivers)
    : IRequestHandler<RegisterDriverCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RegisterDriverCommand request, CancellationToken ct)
    {
        if (await users.EmailExistsAsync(request.Email, ct))
            return Result.Fail<LoginResponse>(AuthErrors.EmailAlreadyRegistered());

        var roleId = await users.GetRoleIdByCodeAsync(RoleCodes.Driver, ct);
        if (roleId is null)
            return Result.Fail<LoginResponse>(AuthErrors.RoleNotConfigured(RoleCodes.Driver));

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone
        };
        await users.CreateAsync(user, ct);
        await users.AssignRoleAsync(user.Id, roleId.Value, ct);

        await drivers.CreateAsync(new Domain.Entities.Driver
        {
            UserId = user.Id,
            Name = $"{request.FirstName} {request.LastName}".Trim(),
            Phone = request.Phone,
            LicenseNumber = request.LicenseNumber,
            VehiclePlate = request.VehiclePlate,
            IsActive = true
        }, ct);

        user = (await users.GetByEmailWithRolesAsync(request.Email, ct))!;
        return Result.Ok(await LoginCommandHandler.BuildLoginResponseAsync(user, users, jwt, drivers, ct));
    }
}

/// <summary>Alias legacy: mismo comportamiento que RegisterCustomerCommand.</summary>
public record RegisterCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<Result<LoginResponse>>;

public class RegisterCommandHandler(IUserRepository users, IJwtTokenService jwt, IDriverRepository drivers)
    : IRequestHandler<RegisterCommand, Result<LoginResponse>>
{
    public Task<Result<LoginResponse>> Handle(RegisterCommand request, CancellationToken ct) =>
        new RegisterCustomerCommandHandler(users, jwt, drivers)
            .Handle(new RegisterCustomerCommand(request.Email, request.Password, request.FirstName, request.LastName, null), ct);
}

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;

public class RefreshTokenCommandHandler(IUserRepository users, IJwtTokenService jwt, IDriverRepository drivers)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = AuthTokenHasher.Hash(request.RefreshToken);
        var stored = await users.GetValidRefreshTokenAsync(hash, ct);
        if (stored is null)
            return Result.Fail<LoginResponse>(AuthErrors.InvalidCredentials());

        await users.RevokeRefreshTokensAsync(stored.UserId, ct);
        return Result.Ok(await LoginCommandHandler.BuildLoginResponseAsync(stored.User, users, jwt, drivers, ct));
    }
}

public record LogoutCommand(Guid UserId) : IRequest<Result>;

public class LogoutCommandHandler(IUserRepository users) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        await users.RevokeRefreshTokensAsync(request.UserId, ct);
        return Result.Ok();
    }
}

public record GetMeQuery(Guid UserId) : IRequest<Result<UserDto>>;

public class GetMeQueryHandler(IUserRepository users, IDriverRepository drivers)
    : IRequestHandler<GetMeQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdWithRolesAsync(request.UserId, ct);
        if (user is null)
            return Result.Fail<UserDto>(AuthErrors.UserNotFound());
        var driver = await drivers.GetByUserIdAsync(user.Id, ct);
        return Result.Ok(await LoginCommandHandler.ToUserDtoAsync(user, driver));
    }
}

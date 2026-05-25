// Rutas de autenticación: registro cliente/repartidor, login, refresh, logout, perfil.
using Ecommerce.Api.Extensions;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Features.Auth;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth").WithTags("Auth");

        // Registro tienda (web/mobile) — asigna rol customer
        auth.MapPost("/register/customer", async (RegisterCustomerRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RegisterCustomerCommand(
                req.Email, req.Password, req.FirstName, req.LastName, req.Phone), ct)).ToHttpResult());

        // Registro app repartidor — crea User + Driver + rol driver
        auth.MapPost("/register/driver", async (RegisterDriverRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RegisterDriverCommand(
                req.Email, req.Password, req.FirstName, req.LastName, req.Phone,
                req.LicenseNumber, req.VehiclePlate), ct)).ToHttpResult());

        // Alias legacy (mismo que register/customer)
        auth.MapPost("/register", async (RegisterRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RegisterCommand(req.Email, req.Password, req.FirstName, req.LastName), ct)).ToHttpResult());

        auth.MapPost("/login", async (LoginRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new LoginCommand(req.Email, req.Password), ct)).ToHttpResult());

        auth.MapPost("/refresh", async (RefreshTokenRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RefreshTokenCommand(req.RefreshToken), ct)).ToHttpResult());

        auth.MapPost("/logout", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new LogoutCommand(userId.Value), ct)).ToHttpResult();
        }).RequireAuthorization();

        auth.MapGet("/me", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetMeQuery(userId.Value), ct)).ToHttpResult();
        }).RequireAuthorization();

        return group;
    }
}

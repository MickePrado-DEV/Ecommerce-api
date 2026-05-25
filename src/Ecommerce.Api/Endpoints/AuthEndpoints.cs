// Rutas de autenticación: registro, login, refresh, logout y perfil.
// Patrón: DTO del body → Command/Query → ISender → ToHttpResult().
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

        // Público: crea usuario y devuelve tokens (409 si email duplicado)
        auth.MapPost("/register", async (RegisterRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RegisterCommand(req.Email, req.Password, req.FirstName, req.LastName), ct)).ToHttpResult());

        // Público: valida credenciales y devuelve access + refresh token (401 si fallan)
        auth.MapPost("/login", async (LoginRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new LoginCommand(req.Email, req.Password), ct)).ToHttpResult());

        // Público: renueva tokens con refresh válido
        auth.MapPost("/refresh", async (RefreshTokenRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RefreshTokenCommand(req.RefreshToken), ct)).ToHttpResult());

        // Requiere JWT: revoca refresh tokens del usuario
        auth.MapPost("/logout", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new LogoutCommand(userId.Value), ct)).ToHttpResult();
        }).RequireAuthorization();

        // Requiere JWT: devuelve perfil del usuario actual
        auth.MapGet("/me", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetMeQuery(userId.Value), ct)).ToHttpResult();
        }).RequireAuthorization();

        return group;
    }
}

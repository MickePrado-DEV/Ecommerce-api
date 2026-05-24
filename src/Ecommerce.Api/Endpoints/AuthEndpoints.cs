using Ecommerce.Api.Filters;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth").WithTags("Auth");

        auth.MapPost("/register", async (RegisterRequest req, IAuthService service, CancellationToken ct) =>
        {
            var result = await service.RegisterAsync(req, ct);
            return Results.Ok(result);
        }).WithValidation<RegisterRequest>();

        auth.MapPost("/login", async (LoginRequest req, IAuthService service, CancellationToken ct) =>
        {
            var result = await service.LoginAsync(req.Email, req.Password, ct);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).WithValidation<LoginRequest>();

        auth.MapPost("/refresh", async (RefreshTokenRequest req, IAuthService service, CancellationToken ct) =>
        {
            var result = await service.RefreshAsync(req.RefreshToken, ct);
            return result is null ? Results.Unauthorized() : Results.Ok(result);
        }).WithValidation<RefreshTokenRequest>();

        auth.MapPost("/logout", async (IAuthService service, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            await service.LogoutAsync(userId.Value, ct);
            return Results.NoContent();
        }).RequireAuthorization();

        auth.MapGet("/me", async (IAuthService service, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            var me = await service.GetMeAsync(userId.Value, ct);
            return me is null ? Results.NotFound() : Results.Ok(me);
        }).RequireAuthorization();

        return group;
    }
}

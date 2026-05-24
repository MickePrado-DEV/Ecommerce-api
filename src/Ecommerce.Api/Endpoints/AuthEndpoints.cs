using Ecommerce.Application.Abstractions;
namespace Ecommerce.Api.Endpoints
{
    public static class AuthEndpoints
    {
        public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
        {
            var auth = group.MapGroup("/auth").WithTags("Auth");

            auth.MapPost("/login", async (LoginRequest req, IAuthService service, CancellationToken ct) =>
            {
                var result = await service.LoginAsync(req.Email, req.Password, ct);
                return result is null ? Results.Unauthorized() : Results.Ok(result);
            });

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
}

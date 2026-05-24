using Ecommerce.Api.Filters;
using Ecommerce.Application.Abstractions;
using Ecommerce.Application.DTOs.Addresses;

namespace Ecommerce.Api.Endpoints;

public static class AddressEndpoints
{
    public static RouteGroupBuilder MapAddressEndpoints(this RouteGroupBuilder group)
    {
        var addresses = group.MapGroup("/addresses").WithTags("Addresses").RequireAuthorization();

        addresses.MapGet("/", async (IAddressService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.ListAsync(userId.Value, ct));
        });

        addresses.MapGet("/{id:guid}", async (Guid id, IAddressService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            var address = await svc.GetAsync(userId.Value, id, ct);
            return address is null ? Results.NotFound() : Results.Ok(address);
        });

        addresses.MapPost("/", async (SaveAddressRequest req, IAddressService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.SaveAsync(userId.Value, req, ct));
        }).WithValidation<SaveAddressRequest>();

        addresses.MapPut("/{id:guid}", async (Guid id, SaveAddressRequest req, IAddressService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await svc.SaveAsync(userId.Value, req with { Id = id }, ct));
        }).WithValidation<SaveAddressRequest>();

        addresses.MapDelete("/{id:guid}", async (Guid id, IAddressService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            await svc.DeleteAsync(userId.Value, id, ct);
            return Results.NoContent();
        });

        addresses.MapPatch("/{id:guid}/default", async (Guid id, IAddressService svc, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            await svc.SetDefaultAsync(userId.Value, id, ct);
            return Results.NoContent();
        });

        return group;
    }
}

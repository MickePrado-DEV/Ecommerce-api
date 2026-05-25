using Ecommerce.Api.Extensions;
using Ecommerce.Application.DTOs.Addresses;
using Ecommerce.Application.Features.Addresses.Commands;
using Ecommerce.Application.Features.Addresses.Queries;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class AddressEndpoints
{
    public static RouteGroupBuilder MapAddressEndpoints(this RouteGroupBuilder group)
    {
        var addresses = group.MapGroup("/addresses").WithTags("Addresses").RequireAuthorization();

        addresses.MapGet("/", async (ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new ListAddressesQuery(userId.Value), ct)).ToHttpResult();
        });

        addresses.MapGet("/{id:guid}", async (Guid id, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new GetAddressQuery(userId.Value, id), ct)).ToHttpResult();
        });

        addresses.MapPost("/", async (SaveAddressRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            var cmd = SaveAddressCommandMapping.FromRequest(userId.Value, req);
            return (await sender.Send(cmd, ct)).ToHttpResult();
        });

        addresses.MapPut("/{id:guid}", async (Guid id, SaveAddressRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            var cmd = SaveAddressCommandMapping.FromRequest(userId.Value, req with { Id = id });
            return (await sender.Send(cmd, ct)).ToHttpResult();
        });

        addresses.MapDelete("/{id:guid}", async (Guid id, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new DeleteAddressCommand(userId.Value, id), ct)).ToHttpResult();
        });

        addresses.MapPatch("/{id:guid}/default", async (Guid id, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new SetDefaultAddressCommand(userId.Value, id), ct)).ToHttpResult();
        });

        return group;
    }
}

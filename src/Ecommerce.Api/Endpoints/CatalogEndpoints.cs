using Ecommerce.Application.Abstractions;

namespace Ecommerce.Api.Endpoints;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogEndpoints(this RouteGroupBuilder group)
    {
        var catalog = group.MapGroup("/catalog").WithTags("Catalog");

        catalog.MapGet("/families", async (ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetFamiliesAsync(ct)));

        catalog.MapGet("/products/{slug}", async (string slug, ICatalogService svc, CancellationToken ct) =>
        {
            var product = await svc.GetProductBySlugAsync(slug, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        catalog.MapGet("/products", async (int page, int pageSize, string? q, ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListProductsAsync(page, pageSize, q, ct)));

        return group;
    }
}
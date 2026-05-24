using Ecommerce.Application.Abstractions;
using Ecommerce.Application.DTOs.Catalog;

namespace Ecommerce.Api.Endpoints;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogEndpoints(this RouteGroupBuilder group)
    {
        var catalog = group.MapGroup("/catalog").WithTags("Catalog");

        catalog.MapGet("/home", async (int? take, ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetHomeAsync(take ?? 12, ct)));

        catalog.MapGet("/covers", async (ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetCoversAsync(ct)));

        catalog.MapGet("/products/latest", async (int? take, ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetLatestProductsAsync(take ?? 12, ct)));

        catalog.MapGet("/families", async (ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetFamiliesAsync(ct)));

        catalog.MapGet("/families/{slug}", async (string slug, ICatalogService svc, CancellationToken ct) =>
        {
            var family = await svc.GetFamilyBySlugAsync(slug, ct);
            return family is null ? Results.NotFound() : Results.Ok(family);
        });

        catalog.MapGet("/categories/{slug}", async (string slug, ICatalogService svc, CancellationToken ct) =>
        {
            var category = await svc.GetCategoryBySlugAsync(slug, ct);
            return category is null ? Results.NotFound() : Results.Ok(category);
        });

        catalog.MapGet("/subcategories/{slug}", async (string slug, ICatalogService svc, CancellationToken ct) =>
        {
            var sub = await svc.GetSubcategoryBySlugAsync(slug, ct);
            return sub is null ? Results.NotFound() : Results.Ok(sub);
        });

        catalog.MapGet("/products/{slug}", async (string slug, ICatalogService svc, CancellationToken ct) =>
        {
            var product = await svc.GetProductBySlugAsync(slug, ct);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        catalog.MapGet("/products", async (
            int page, int pageSize, Guid? familyId, Guid? categoryId, Guid? subCategoryId,
            string? q, string? sort, ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListProductsAsync(
                new CatalogProductQuery(page, pageSize, familyId, categoryId, subCategoryId, q, sort), ct)));

        catalog.MapGet("/search", async (string q, int page, int pageSize, ICatalogService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListProductsAsync(new CatalogProductQuery(page, pageSize, Search: q), ct)));

        return group;
    }
}

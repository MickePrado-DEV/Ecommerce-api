// Catálogo público (solo lectura). Cada ruta envía una Query a MediatR.
using Ecommerce.Api.Extensions;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.Features.Catalog.Queries;
using MediatR;

namespace Ecommerce.Api.Endpoints;

public static class CatalogEndpoints
{
    public static RouteGroupBuilder MapCatalogEndpoints(this RouteGroupBuilder group)
    {
        var catalog = group.MapGroup("/catalog").WithTags("Catalog");

        // Home: portadas + productos destacados
        catalog.MapGet("/home", async (int? take, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetCatalogHomeQuery(take ?? 12), ct)).ToHttpResult());

        catalog.MapGet("/covers", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetCoversQuery(), ct)).ToHttpResult());

        catalog.MapGet("/products/latest", async (int? take, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetLatestProductsQuery(take ?? 12), ct)).ToHttpResult());

        catalog.MapGet("/families", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetFamiliesQuery(), ct)).ToHttpResult());

        catalog.MapGet("/families/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetFamilyBySlugQuery(slug), ct)).ToHttpResult());

        catalog.MapGet("/categories/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetCategoryBySlugQuery(slug), ct)).ToHttpResult());

        catalog.MapGet("/subcategories/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSubcategoryBySlugQuery(slug), ct)).ToHttpResult());

        // Detalle de producto con variantes y stock disponible
        catalog.MapGet("/products/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetProductBySlugQuery(slug), ct)).ToHttpResult());

        // Listado con filtros: familia, categoría, búsqueda, orden
        catalog.MapGet("/products", async (
            int page, int pageSize, Guid? familyId, Guid? categoryId, Guid? subCategoryId,
            string? q, string? sort, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductsQuery(
                new CatalogProductQuery(page, pageSize, familyId, categoryId, subCategoryId, q, sort)), ct)).ToHttpResult());

        catalog.MapGet("/search", async (string q, int page, int pageSize, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductsQuery(
                new CatalogProductQuery(page, pageSize, Search: q)), ct)).ToHttpResult());

        return group;
    }
}

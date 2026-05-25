// Catálogo público (solo lectura). Cada ruta envía una Query a MediatR.
using Ecommerce.Api.Extensions;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.DTOs.Reviews;
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

        // Detalle de producto con variantes, opciones y stock disponible
        catalog.MapGet("/products/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetProductBySlugQuery(slug), ct)).ToHttpResult());

        catalog.MapGet("/products/{slug}/reviews", async (string slug, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetProductReviewsQuery(slug), ct)).ToHttpResult());

        catalog.MapPost("/products/{slug}/reviews", async (
            string slug, CreateProductReviewRequest req, ISender sender, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = ctx.GetUserId();
            if (userId is null) return Results.Unauthorized();
            return (await sender.Send(new CreateProductReviewCommand(
                userId.Value, slug, req.Rating, req.Title, req.Comment), ct)).ToHttpResult();
        }).RequireAuthorization();

        catalog.MapPost("/products/{slug}/resolve-variant", async (
            string slug, ResolveVariantRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ResolveProductVariantQuery(slug, req.OptionValueIds), ct)).ToHttpResult());

        // Listado con filtros: familia, categoría, búsqueda, orden, optionValueIds
        catalog.MapGet("/products", async (
            int page, int pageSize, Guid? familyId, Guid? categoryId, Guid? subCategoryId,
            string? q, string? sort, string? optionValueIds, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductsQuery(
                new CatalogProductQuery(
                    page, pageSize, familyId, categoryId, subCategoryId, q, sort,
                    ParseGuidList(optionValueIds))), ct)).ToHttpResult());

        catalog.MapGet("/search", async (string q, int page, int pageSize, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ListProductsQuery(
                new CatalogProductQuery(page, pageSize, Search: q)), ct)).ToHttpResult());

        return group;
    }

    private static IReadOnlyList<Guid>? ParseGuidList(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return null;
        var list = new List<Guid>();
        foreach (var part in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Guid.TryParse(part, out var id))
                list.Add(id);
        }
        return list.Count > 0 ? list : null;
    }
}

public record ResolveVariantRequest(IReadOnlyList<Guid> OptionValueIds);

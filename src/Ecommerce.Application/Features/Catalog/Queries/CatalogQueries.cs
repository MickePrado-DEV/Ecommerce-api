// Queries de catálogo público: lecturas vía ICatalogReadRepository (proyecciones EF, sin tracking).
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;
using Ecommerce.Application.DTOs.Reviews;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Catalog.Queries;

public record GetFamiliesQuery : IRequest<Result<IReadOnlyList<FamilyDto>>>;

public class GetFamiliesQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetFamiliesQuery, Result<IReadOnlyList<FamilyDto>>>
{
    public async Task<Result<IReadOnlyList<FamilyDto>>> Handle(GetFamiliesQuery request, CancellationToken ct) =>
        Result.Ok(await repo.GetFamiliesTreeAsync(ct));
}

public record GetCatalogHomeQuery(int Take = 12) : IRequest<Result<CatalogHomeDto>>;

public class GetCatalogHomeQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetCatalogHomeQuery, Result<CatalogHomeDto>>
{
    public async Task<Result<CatalogHomeDto>> Handle(GetCatalogHomeQuery request, CancellationToken ct)
    {
        var covers = await repo.GetActiveCoversAsync(ct);
        var latest = await repo.GetLatestProductsAsync(request.Take, ct);
        return Result.Ok(new CatalogHomeDto(covers, latest));
    }
}

public record GetCoversQuery : IRequest<Result<IReadOnlyList<CoverDto>>>;

public class GetCoversQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetCoversQuery, Result<IReadOnlyList<CoverDto>>>
{
    public async Task<Result<IReadOnlyList<CoverDto>>> Handle(GetCoversQuery request, CancellationToken ct) =>
        Result.Ok(await repo.GetActiveCoversAsync(ct));
}

public record GetLatestProductsQuery(int Take = 12) : IRequest<Result<IReadOnlyList<ProductListItemDto>>>;

public class GetLatestProductsQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetLatestProductsQuery, Result<IReadOnlyList<ProductListItemDto>>>
{
    public async Task<Result<IReadOnlyList<ProductListItemDto>>> Handle(GetLatestProductsQuery request, CancellationToken ct) =>
        Result.Ok(await repo.GetLatestProductsAsync(request.Take, ct));
}

public record GetFamilyBySlugQuery(string Slug) : IRequest<Result<FamilyDetailDto>>;

public class GetFamilyBySlugQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetFamilyBySlugQuery, Result<FamilyDetailDto>>
{
    public async Task<Result<FamilyDetailDto>> Handle(GetFamilyBySlugQuery request, CancellationToken ct)
    {
        var family = await repo.GetFamilyBySlugAsync(request.Slug, ct);
        return family is null
            ? Result.Fail<FamilyDetailDto>(CatalogErrors.FamilyNotFound(request.Slug))
            : Result.Ok(family);
    }
}

public record GetCategoryBySlugQuery(string Slug) : IRequest<Result<CategoryDetailDto>>;

public class GetCategoryBySlugQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetCategoryBySlugQuery, Result<CategoryDetailDto>>
{
    public async Task<Result<CategoryDetailDto>> Handle(GetCategoryBySlugQuery request, CancellationToken ct)
    {
        var category = await repo.GetCategoryBySlugAsync(request.Slug, ct);
        return category is null
            ? Result.Fail<CategoryDetailDto>(CatalogErrors.CategoryNotFound(request.Slug))
            : Result.Ok(category);
    }
}

public record GetSubcategoryBySlugQuery(string Slug) : IRequest<Result<SubcategoryDetailDto>>;

public class GetSubcategoryBySlugQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetSubcategoryBySlugQuery, Result<SubcategoryDetailDto>>
{
    public async Task<Result<SubcategoryDetailDto>> Handle(GetSubcategoryBySlugQuery request, CancellationToken ct)
    {
        var sub = await repo.GetSubcategoryBySlugAsync(request.Slug, ct);
        return sub is null
            ? Result.Fail<SubcategoryDetailDto>(CatalogErrors.SubcategoryNotFound(request.Slug))
            : Result.Ok(sub);
    }
}

public record GetProductBySlugQuery(string Slug) : IRequest<Result<ProductDetailDto>>;

public class GetProductBySlugQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetProductBySlugQuery, Result<ProductDetailDto>>
{
    public async Task<Result<ProductDetailDto>> Handle(GetProductBySlugQuery request, CancellationToken ct)
    {
        var product = await repo.GetProductBySlugAsync(request.Slug, ct);
        return product is null
            ? Result.Fail<ProductDetailDto>(CatalogErrors.ProductNotFound(request.Slug))
            : Result.Ok(product);
    }
}

public record ListProductsQuery(CatalogProductQuery Query) : IRequest<Result<PagedResult<ProductListItemDto>>>;

public class ListProductsQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<ListProductsQuery, Result<PagedResult<ProductListItemDto>>>
{
    public async Task<Result<PagedResult<ProductListItemDto>>> Handle(ListProductsQuery request, CancellationToken ct) =>
        Result.Ok(await repo.ListProductsAsync(request.Query, ct));
}

public record GetCatalogFilterOptionsQuery(Guid? FamilyId, Guid? CategoryId, Guid? SubCategoryId)
    : IRequest<Result<IReadOnlyList<CatalogOptionDto>>>;

public class GetCatalogFilterOptionsQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<GetCatalogFilterOptionsQuery, Result<IReadOnlyList<CatalogOptionDto>>>
{
    public async Task<Result<IReadOnlyList<CatalogOptionDto>>> Handle(
        GetCatalogFilterOptionsQuery request,
        CancellationToken ct) =>
        Result.Ok(await repo.GetFilterOptionsAsync(request.FamilyId, request.CategoryId, request.SubCategoryId, ct));
}

public record ResolveProductVariantQuery(string Slug, IReadOnlyList<Guid> OptionValueIds)
    : IRequest<Result<ResolvedVariantDto>>;

public class ResolveProductVariantQueryHandler(ICatalogReadRepository repo)
    : IRequestHandler<ResolveProductVariantQuery, Result<ResolvedVariantDto>>
{
    public async Task<Result<ResolvedVariantDto>> Handle(ResolveProductVariantQuery request, CancellationToken ct)
    {
        if (request.OptionValueIds.Count == 0)
            return Result.Fail<ResolvedVariantDto>(CatalogErrors.VariantNotResolved(request.Slug));

        var variant = await repo.ResolveVariantAsync(request.Slug, request.OptionValueIds, ct);
        return variant is null
            ? Result.Fail<ResolvedVariantDto>(CatalogErrors.VariantNotResolved(request.Slug))
            : Result.Ok(variant);
    }
}

public record GetProductReviewsQuery(string Slug) : IRequest<Result<ProductReviewsPageDto>>;

public class GetProductReviewsQueryHandler(IProductReviewRepository reviews)
    : IRequestHandler<GetProductReviewsQuery, Result<ProductReviewsPageDto>>
{
    public async Task<Result<ProductReviewsPageDto>> Handle(GetProductReviewsQuery request, CancellationToken ct)
    {
        var product = await reviews.GetActiveProductBySlugAsync(request.Slug, ct);
        if (product is null)
            return Result.Fail<ProductReviewsPageDto>(ReviewErrors.ProductNotFound(request.Slug));

        var summary = await reviews.GetSummaryByProductIdAsync(product.Id, ct)
                      ?? new ProductReviewSummaryDto(0, 0);
        var items = await reviews.ListApprovedByProductIdAsync(product.Id, ct);
        return Result.Ok(new ProductReviewsPageDto(summary, items));
    }
}

public record GetProductReviewEligibilityQuery(Guid UserId, string Slug)
    : IRequest<Result<ProductReviewEligibilityDto>>;

public class GetProductReviewEligibilityQueryHandler(IProductReviewRepository reviews)
    : IRequestHandler<GetProductReviewEligibilityQuery, Result<ProductReviewEligibilityDto>>
{
    public async Task<Result<ProductReviewEligibilityDto>> Handle(
        GetProductReviewEligibilityQuery request, CancellationToken ct)
    {
        var product = await reviews.GetActiveProductBySlugAsync(request.Slug, ct);
        if (product is null)
            return Result.Fail<ProductReviewEligibilityDto>(ReviewErrors.ProductNotFound(request.Slug));

        var already = await reviews.UserHasReviewedAsync(request.UserId, product.Id, ct);
        var delivered = await reviews.UserHasDeliveredProductAsync(request.UserId, product.Id, ct);
        var canReview = delivered && !already;

        string? message = null;
        if (already)
            message = "Ya publicaste una reseña para este producto.";
        else if (!delivered)
            message = "Solo puedes reseñar después de recibir el producto (pedido entregado).";

        return Result.Ok(new ProductReviewEligibilityDto(canReview, already, delivered, message));
    }
}

public record CreateProductReviewCommand(Guid UserId, string Slug, int Rating, string? Title, string Comment)
    : IRequest<Result<ProductReviewDto>>;

public class CreateProductReviewCommandHandler(
    IProductReviewRepository reviews,
    IUserRepository users) : IRequestHandler<CreateProductReviewCommand, Result<ProductReviewDto>>
{
    public async Task<Result<ProductReviewDto>> Handle(CreateProductReviewCommand request, CancellationToken ct)
    {
        var product = await reviews.GetActiveProductBySlugAsync(request.Slug, ct);
        if (product is null)
            return Result.Fail<ProductReviewDto>(ReviewErrors.ProductNotFound(request.Slug));

        if (await reviews.UserHasReviewedAsync(request.UserId, product.Id, ct))
            return Result.Fail<ProductReviewDto>(ReviewErrors.AlreadyReviewed());

        if (!await reviews.UserHasDeliveredProductAsync(request.UserId, product.Id, ct))
            return Result.Fail<ProductReviewDto>(ReviewErrors.NotEligibleForReview());

        var user = await users.GetByIdWithRolesAsync(request.UserId, ct);
        if (user is null)
            return Result.Fail<ProductReviewDto>(new Error("Usuario no encontrado"));

        var review = new ProductReview
        {
            ProductId = product.Id,
            UserId = request.UserId,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment,
            IsApproved = true
        };
        await reviews.AddAsync(review, ct);

        return Result.Ok(new ProductReviewDto(
            review.Id,
            $"{user.FirstName} {user.LastName}",
            review.Rating,
            review.Title,
            review.Comment,
            review.CreatedAt));
    }
}

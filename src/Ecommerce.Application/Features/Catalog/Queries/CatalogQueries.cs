using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Catalog;
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

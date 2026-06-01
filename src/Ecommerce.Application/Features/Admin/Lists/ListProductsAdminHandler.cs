using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Admin;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record ListProductsAdminQuery(int Page, int PageSize, string? Search = null, string? SortBy = null, string SortDirection = "asc")
    : IRequest<Result<PagedResult<ProductAdminDto>>>;

public class ListProductsAdminQueryHandler(IAdminCatalogRepository repo)
    : IRequestHandler<ListProductsAdminQuery, Result<PagedResult<ProductAdminDto>>>
{
    private static readonly string[] SortKeys = ["name", "slug", "basePrice", "createdAt"];

    public async Task<Result<PagedResult<ProductAdminDto>>> Handle(ListProductsAdminQuery request, CancellationToken ct)
    {
        var paging = PaginationRules.NormalizeOrDefault(request.Page, request.PageSize);
        if (paging.IsFailed) return Result.Fail<PagedResult<ProductAdminDto>>(paging.Errors);
        var sort = PaginationRules.ValidateSort(request.SortBy, request.SortDirection, SortKeys);
        if (sort.IsFailed) return Result.Fail<PagedResult<ProductAdminDto>>(sort.Errors);

        var (page, pageSize) = paging.Value;
        var result = await repo.ListProductsAsync(page, pageSize, request.Search, request.SortBy, request.SortDirection, ct);
        var items = result.Items.Select(p => new ProductAdminDto(
            p.Id, p.SubcategoryId, p.Name, p.Slug, p.Description, p.BasePrice, p.IsActive)).ToList();
        return Result.Ok(PaginationRules.Create(items, result.Total, page, pageSize));
    }
}

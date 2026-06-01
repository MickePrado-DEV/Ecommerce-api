using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Inventory;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record ListInventoryAdminQuery(int Page, int PageSize, string? Search = null, string? SortBy = null, string SortDirection = "asc")
    : IRequest<Result<PagedResult<InventoryDto>>>;

public class ListInventoryAdminQueryHandler(IInventoryRepository inventory)
    : IRequestHandler<ListInventoryAdminQuery, Result<PagedResult<InventoryDto>>>
{
    private static readonly string[] SortKeys = ["sku", "productName", "quantityOnHand", "available"];

    public async Task<Result<PagedResult<InventoryDto>>> Handle(ListInventoryAdminQuery request, CancellationToken ct)
    {
        var paging = PaginationRules.NormalizeOrDefault(request.Page, request.PageSize);
        if (paging.IsFailed) return Result.Fail<PagedResult<InventoryDto>>(paging.Errors);
        var sort = PaginationRules.ValidateSort(request.SortBy, request.SortDirection, SortKeys);
        if (sort.IsFailed) return Result.Fail<PagedResult<InventoryDto>>(sort.Errors);

        var (page, pageSize) = paging.Value;
        var (rows, total) = await inventory.ListPagedAsync(page, pageSize, request.Search, request.SortBy, request.SortDirection, ct);
        var items = rows.Select(i => new InventoryDto(
            i.VariantId, i.Variant.Sku, i.Variant.Product.Name,
            i.QuantityOnHand, i.QuantityReserved,
            i.QuantityOnHand - i.QuantityReserved)).ToList();
        return Result.Ok(PaginationRules.Create(items, total, page, pageSize));
    }
}

using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Emums;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record ListOrdersAdminQuery(int Page, int PageSize, OrderStatus? Status, string? SortBy = null, string SortDirection = "desc")
    : IRequest<Result<PagedResult<OrderSummaryDto>>>;

public class ListOrdersAdminQueryHandler(IOrderRepository orders)
    : IRequestHandler<ListOrdersAdminQuery, Result<PagedResult<OrderSummaryDto>>>
{
    private static readonly string[] SortKeys = ["orderNumber", "createdAt", "total", "status"];

    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(ListOrdersAdminQuery request, CancellationToken ct)
    {
        var paging = PaginationRules.NormalizeOrDefault(request.Page, request.PageSize);
        if (paging.IsFailed) return Result.Fail<PagedResult<OrderSummaryDto>>(paging.Errors);
        var sort = PaginationRules.ValidateSort(request.SortBy, request.SortDirection, SortKeys);
        if (sort.IsFailed) return Result.Fail<PagedResult<OrderSummaryDto>>(sort.Errors);

        var (page, pageSize) = paging.Value;
        var result = await orders.ListAdminAsync(page, pageSize, request.Status, request.SortBy, request.SortDirection, ct);
        var items = result.Items.Select(o => new OrderSummaryDto(
            o.Id, o.OrderNumber, o.Status.ToString(), o.Total, o.CreatedAt)).ToList();
        return Result.Ok(PaginationRules.Create(items, result.Total, page, pageSize));
    }
}

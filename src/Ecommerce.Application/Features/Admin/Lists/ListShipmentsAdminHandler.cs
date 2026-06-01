using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Shipments;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record ListShipmentsAdminQuery(int Page, int PageSize, string? Search = null, string? SortBy = null, string SortDirection = "desc")
    : IRequest<Result<PagedResult<ShipmentSummaryDto>>>;

public class ListShipmentsAdminQueryHandler(IShipmentRepository shipments)
    : IRequestHandler<ListShipmentsAdminQuery, Result<PagedResult<ShipmentSummaryDto>>>
{
    private static readonly string[] SortKeys = ["orderNumber", "status", "trackingNumber", "createdAt"];

    public async Task<Result<PagedResult<ShipmentSummaryDto>>> Handle(ListShipmentsAdminQuery request, CancellationToken ct)
    {
        var paging = PaginationRules.NormalizeOrDefault(request.Page, request.PageSize);
        if (paging.IsFailed) return Result.Fail<PagedResult<ShipmentSummaryDto>>(paging.Errors);
        var sort = PaginationRules.ValidateSort(request.SortBy, request.SortDirection, SortKeys);
        if (sort.IsFailed) return Result.Fail<PagedResult<ShipmentSummaryDto>>(sort.Errors);

        var (page, pageSize) = paging.Value;
        var (list, total) = await shipments.ListAsync(page, pageSize, request.Search, request.SortBy, request.SortDirection, ct);
        var items = list.Select(s => new ShipmentSummaryDto(
            s.Id, s.OrderId, s.Order.OrderNumber, s.Status.ToString(), s.TrackingNumber,
            s.Driver?.Name, s.CreatedAt)).ToList();
        return Result.Ok(PaginationRules.Create(items, total, page, pageSize));
    }
}

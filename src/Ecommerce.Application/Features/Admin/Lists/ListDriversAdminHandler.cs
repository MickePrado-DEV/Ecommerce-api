using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Shipments;
using Ecommerce.Application.Features.Admin;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record ListDriversAdminQuery(int Page, int PageSize, string? Search = null, string? SortBy = null, string SortDirection = "asc")
    : IRequest<Result<PagedResult<DriverDto>>>;

public class ListDriversAdminQueryHandler(IShipmentRepository shipments)
    : IRequestHandler<ListDriversAdminQuery, Result<PagedResult<DriverDto>>>
{
    private static readonly string[] SortKeys = ["name", "phone", "vehiclePlate", "isActive"];

    public async Task<Result<PagedResult<DriverDto>>> Handle(ListDriversAdminQuery request, CancellationToken ct)
    {
        var paging = PaginationRules.NormalizeOrDefault(request.Page, request.PageSize);
        if (paging.IsFailed) return Result.Fail<PagedResult<DriverDto>>(paging.Errors);
        var sort = PaginationRules.ValidateSort(request.SortBy, request.SortDirection, SortKeys);
        if (sort.IsFailed) return Result.Fail<PagedResult<DriverDto>>(sort.Errors);

        var (page, pageSize) = paging.Value;
        var (rows, total) = await shipments.ListDriversPagedAsync(
            page, pageSize, request.Search, request.SortBy, request.SortDirection, ct);
        var items = rows.Select(d => AdminDriverMapping.Map(d)).ToList();
        return Result.Ok(PaginationRules.Create(items, total, page, pageSize));
    }
}

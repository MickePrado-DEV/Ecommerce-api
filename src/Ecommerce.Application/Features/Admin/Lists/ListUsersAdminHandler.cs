using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Admin;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record ListUsersAdminQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = null,
    string SortDirection = "desc")
    : IRequest<Result<PagedResult<UserAdminDto>>>;

public class ListUsersAdminQueryHandler(IAdminUserRepository repo)
    : IRequestHandler<ListUsersAdminQuery, Result<PagedResult<UserAdminDto>>>
{
    private static readonly string[] SortKeys = ["email", "firstName", "lastName", "createdAt", "isActive"];

    public async Task<Result<PagedResult<UserAdminDto>>> Handle(ListUsersAdminQuery request, CancellationToken ct)
    {
        var paging = PaginationRules.NormalizeOrDefault(request.Page, request.PageSize);
        if (paging.IsFailed) return Result.Fail<PagedResult<UserAdminDto>>(paging.Errors);
        var sort = PaginationRules.ValidateSort(request.SortBy, request.SortDirection, SortKeys);
        if (sort.IsFailed) return Result.Fail<PagedResult<UserAdminDto>>(sort.Errors);

        var (page, pageSize) = paging.Value;
        var (items, total) = await repo.ListAsync(
            page, pageSize, request.Search, request.SortBy, request.SortDirection, ct);
        var dtos = items.Select(AdminUserMapping.MapUser).ToList();
        return Result.Ok(PaginationRules.Create(dtos, total, page, pageSize));
    }
}

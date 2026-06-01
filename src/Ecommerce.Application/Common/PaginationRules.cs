using Ecommerce.Domain.Admin;
using FluentResults;

namespace Ecommerce.Application.Common;

public static class PaginationRules
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static Result<(int Page, int PageSize)> Normalize(int page, int pageSize)
    {
        if (page < 1)
            return Result.Fail<(int, int)>(AdminErrors.Validation("page debe ser >= 1."));
        if (pageSize < 1)
            return Result.Fail<(int, int)>(AdminErrors.Validation("pageSize debe ser >= 1."));
        if (pageSize > MaxPageSize)
            return Result.Fail<(int, int)>(AdminErrors.Validation($"pageSize no puede superar {MaxPageSize}."));
        return Result.Ok((page, pageSize));
    }

    public static Result<(int Page, int PageSize)> NormalizeOrDefault(int page, int pageSize, int defaultPageSize = DefaultPageSize)
    {
        var p = page < 1 ? 1 : page;
        var ps = pageSize < 1 ? defaultPageSize : pageSize;
        return Normalize(p, ps);
    }

    public static PagedResult<T> Create<T>(IReadOnlyList<T> items, int total, int page, int pageSize) =>
        new(items, total, page, pageSize);

    public static Result ValidateSort(string? sortBy, string? sortDirection, IReadOnlyCollection<string> allowedKeys)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return Result.Ok();
        if (!allowedKeys.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            return Result.Fail(AdminErrors.Validation($"sortBy inválido: {sortBy}."));
        if (!string.IsNullOrWhiteSpace(sortDirection)
            && sortDirection is not ("asc" or "desc"))
            return Result.Fail(AdminErrors.Validation("sortDirection debe ser asc o desc."));
        return Result.Ok();
    }
}

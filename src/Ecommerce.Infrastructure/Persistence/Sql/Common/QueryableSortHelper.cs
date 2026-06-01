using System.Linq.Expressions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Common;

public static class QueryableSortHelper
{
    public static IQueryable<T> OrderByField<T>(
        this IQueryable<T> query,
        string? sortBy,
        string sortDirection,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> fields,
        Expression<Func<T, object>> defaultKey)
    {
        var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(sortBy)
            || !fields.TryGetValue(sortBy, out var key))
            return desc ? query.OrderByDescending(defaultKey) : query.OrderBy(defaultKey);

        return desc ? query.OrderByDescending(key) : query.OrderBy(key);
    }
}

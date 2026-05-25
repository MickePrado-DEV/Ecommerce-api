namespace Ecommerce.Api.Models;

public sealed record ApiErrorItem(string Message, string? Code = null, string? PropertyName = null);

public sealed record ApiErrorResponse(IReadOnlyList<ApiErrorItem> Errors);

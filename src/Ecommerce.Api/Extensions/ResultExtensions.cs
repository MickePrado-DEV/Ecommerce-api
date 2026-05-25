// Convierte FluentResults (Result / Result<T>) en respuestas HTTP de Minimal API.
using Ecommerce.Api.Models;
using FluentResults;

namespace Ecommerce.Api.Extensions;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? Results.NoContent() : ToErrorResult(result);

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : ToErrorResult(result);

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : ToErrorResult(result);

    private static IResult ToErrorResult(IResultBase result)
    {
        var items = result.Errors.Select(e => new ApiErrorItem(
            e.Message,
            e.Metadata.GetValueOrDefault("Code")?.ToString(),
            e.Metadata.GetValueOrDefault("PropertyName")?.ToString())).ToList();

        var status = MapStatusCode(items);
        return Results.Json(new ApiErrorResponse(items), statusCode: (int)status);
    }

    private static System.Net.HttpStatusCode MapStatusCode(IReadOnlyList<ApiErrorItem> errors)
    {
        var codes = errors.Select(e => e.Code).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
        if (codes.Count == 0)
            return System.Net.HttpStatusCode.BadRequest;

        if (codes.Any(c => c is "NotFound" or "Address.NotFound" or "Catalog.NotFound" or "Order.NotFound"))
            return System.Net.HttpStatusCode.NotFound;

        if (codes.Any(c => c is "Unauthorized"))
            return System.Net.HttpStatusCode.Unauthorized;

        if (codes.Any(c => c is "Forbidden"))
            return System.Net.HttpStatusCode.Forbidden;

        if (codes.Any(c => c is "Conflict" or "InsufficientStock" or "Auth.Conflict"))
            return System.Net.HttpStatusCode.Conflict;

        if (codes.Any(c => c is "Validation" or "Address.Validation"))
            return System.Net.HttpStatusCode.BadRequest;

        if (codes.Any(c => c is "Database.SchemaMismatch" or "Database.Unavailable"))
            return System.Net.HttpStatusCode.ServiceUnavailable;

        if (codes.Any(c => c is "Database.Timeout"))
            return System.Net.HttpStatusCode.GatewayTimeout;

        return System.Net.HttpStatusCode.BadRequest;
    }
}

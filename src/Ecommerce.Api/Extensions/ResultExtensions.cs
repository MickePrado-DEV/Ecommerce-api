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
        var code = result.Errors.FirstOrDefault()?.Metadata.GetValueOrDefault("Code")?.ToString();

        return code switch
        {
            "Address.NotFound" or "Catalog.NotFound" or "NotFound" => Results.NotFound(new { errors = FormatErrors(result) }),
            "Order.NotFound" => Results.NotFound(new { errors = FormatErrors(result) }),
            "Validation" or "Address.Validation" => Results.ValidationProblem(ToValidationDictionary(result)),
            "Conflict" or "InsufficientStock" or "Auth.Conflict" => Results.Conflict(new { errors = FormatErrors(result) }),
            "Unauthorized" => Results.Unauthorized(),
            _ => Results.BadRequest(new { errors = FormatErrors(result) })
        };
    }

    private static Dictionary<string, string[]> ToValidationDictionary(IResultBase result)
    {
        var dict = new Dictionary<string, string[]>();
        foreach (var error in result.Errors)
        {
            var prop = error.Metadata.GetValueOrDefault("PropertyName")?.ToString() ?? "request";
            if (!dict.ContainsKey(prop))
                dict[prop] = [error.Message];
            else
                dict[prop] = dict[prop].Concat([error.Message]).ToArray();
        }
        return dict;
    }

    private static IEnumerable<object> FormatErrors(IResultBase result) =>
        result.Errors.Select(e => new
        {
            message = e.Message,
            code = e.Metadata.GetValueOrDefault("Code"),
            propertyName = e.Metadata.GetValueOrDefault("PropertyName")
        });
}

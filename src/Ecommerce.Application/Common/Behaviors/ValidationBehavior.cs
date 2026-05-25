// Pipeline MediatR: ejecuta FluentValidation antes de cada handler.
using FluentResults;
using FluentValidation;
using MediatR;

namespace Ecommerce.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Si no hay validador para este command/query, pasa directo al handler
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Devuelve Result.Fail con código Validation → el endpoint mapea a HTTP 400
        return ResultFactory.CreateFailure<TResponse>(failures);
    }
}

internal static class ResultFactory
{
    public static TResponse CreateFailure<TResponse>(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
    {
        var errors = failures
            .Select(f => new Error(f.ErrorMessage)
                .WithMetadata("Code", "Validation")
                .WithMetadata("PropertyName", f.PropertyName))
            .ToList();

        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Fail(errors);

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var method = typeof(Result)
                .GetMethods()
                .First(m => m.Name == nameof(Result.Fail) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1)
                .MakeGenericMethod(valueType);
            return (TResponse)method.Invoke(null, [errors])!;
        }

        throw new ValidationException(failures);
    }
}

// Middleware global: convierte excepciones en respuestas JSON con código HTTP adecuado.
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception {ExceptionType}", ex.GetType().Name);
            await ApiExceptionMapper.WriteAsync(context, ex, env);
        }
    }
}

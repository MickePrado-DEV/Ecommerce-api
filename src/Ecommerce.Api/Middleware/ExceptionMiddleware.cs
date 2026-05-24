using Ecommerce.Domain.Exceptions;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text.Json;

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
            logger.LogError(ex, "Unhandled exception");
            await HandleAsync(context, ex);
        }
    }

    private Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, message) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, ex.Message),
            InsufficientStockException => (HttpStatusCode.Conflict, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            _ => (HttpStatusCode.InternalServerError, GetInternalErrorMessage(ex))
        };

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }

    private string GetInternalErrorMessage(Exception ex)
    {
        if (!env.IsDevelopment()) return "Error interno del servidor";

        if (IsSchemaMismatch(ex))
            return "Esquema de base de datos desactualizado. Detén la API, reiníciala (recreará tablas) o borra la BD 'ecommerce' en LocalDB.";

        return ex.Message;
    }

    private static bool IsSchemaMismatch(Exception ex) =>
        ex switch
        {
            SqlException { Number: 207 } => true,
            { InnerException: not null } => IsSchemaMismatch(ex.InnerException),
            _ => false
        };
}

using Ecommerce.Api.Models;
using Ecommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Ecommerce.Api.Middleware;

public static class ApiExceptionMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static (HttpStatusCode Status, ApiErrorResponse Body) Map(Exception ex, IHostEnvironment env)
    {
        if (ex is InvalidOperationException opEx)
        {
            var (status, message) = MapInvalidOperation(opEx);
            return (status, Single(message, status == HttpStatusCode.Unauthorized ? "Unauthorized" : "Validation"));
        }

        return ex switch
        {
            ValidationException fv => (
                HttpStatusCode.BadRequest,
                new ApiErrorResponse(fv.Errors.Select(e => new ApiErrorItem(
                    e.ErrorMessage,
                    "Validation",
                    ToCamelCase(e.PropertyName))).ToList())),

            NotFoundException nf => (
                HttpStatusCode.NotFound,
                Single(nf.Message, "NotFound")),

            InsufficientStockException stock => (
                HttpStatusCode.Conflict,
                Single(stock.Message, "InsufficientStock")),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                Single("No autorizado.", "Unauthorized")),

            ArgumentException arg => (
                HttpStatusCode.BadRequest,
                Single(arg.Message, "Validation")),

            BadHttpRequestException bad => (
                HttpStatusCode.BadRequest,
                Single(bad.Message, "BadRequest")),

            JsonException json => (
                HttpStatusCode.BadRequest,
                Single("JSON inválido en el cuerpo de la petición.", "BadRequest", detail: env.IsDevelopment() ? json.Message : null)),

            DbUpdateException db => MapDatabase(db, env),

            SqlException sql => MapSql(sql, env),

            _ when IsSchemaMismatch(ex) => (
                HttpStatusCode.ServiceUnavailable,
                Single(
                    "La base de datos no tiene el esquema actual. Reinicia la API para actualizarla o ejecuta scriptsSql/migrate-address-payment-options.sql.",
                    "Database.SchemaMismatch")),

            _ => (
                HttpStatusCode.InternalServerError,
                Single(
                    env.IsDevelopment() ? ex.Message : "Ocurrió un error inesperado. Intenta de nuevo o contacta soporte.",
                    "Internal.ServerError",
                    detail: env.IsDevelopment() ? ex.GetType().Name : null))
        };
    }

    private static (HttpStatusCode, ApiErrorResponse) MapDatabase(DbUpdateException ex, IHostEnvironment env)
    {
        if (ex.InnerException is SqlException sql)
            return MapSql(sql, env);

        if (IsSchemaMismatch(ex))
            return (HttpStatusCode.ServiceUnavailable, Single(
                "La base de datos no tiene el esquema actual. Reinicia la API para actualizarla.",
                "Database.SchemaMismatch"));

        return (HttpStatusCode.BadRequest, Single(
            "No se pudo guardar los datos. Revisa los campos enviados.",
            "Database.Constraint",
            detail: env.IsDevelopment() ? ex.Message : null));
    }

    private static (HttpStatusCode, ApiErrorResponse) MapSql(SqlException sql, IHostEnvironment env)
    {
        var (status, code, message) = sql.Number switch
        {
            207 => (HttpStatusCode.ServiceUnavailable, "Database.SchemaMismatch",
                "Columnas de base de datos faltantes. Reinicia la API o ejecuta la migración SQL."),
            2627 or 2601 => (HttpStatusCode.Conflict, "Conflict",
                "Ya existe un registro con esos datos."),
            547 => MapForeignKeyViolation(sql, env),
            515 => (HttpStatusCode.BadRequest, "Validation",
                "Faltan campos obligatorios en la base de datos."),
            -2 => (HttpStatusCode.GatewayTimeout, "Database.Timeout",
                "La base de datos tardó demasiado en responder."),
            4060 => (HttpStatusCode.ServiceUnavailable, "Database.Unavailable",
                "No se pudo abrir la base de datos. Verifica que SQL Server esté activo."),
            18456 => (HttpStatusCode.ServiceUnavailable, "Database.Unavailable",
                "No se pudo autenticar con la base de datos."),
            _ => (HttpStatusCode.InternalServerError, "Database.Error",
                env.IsDevelopment() ? sql.Message : "Error al acceder a la base de datos.")
        };

        return (status, Single(message, code, detail: env.IsDevelopment() ? $"SQL {sql.Number}" : null));
    }

    private static (HttpStatusCode, string, string) MapForeignKeyViolation(SqlException sql, IHostEnvironment env)
    {
        var msg = GetSqlErrorText(sql);
        if (IsAddressUserFk(msg))
        {
            return (HttpStatusCode.Unauthorized, "Unauthorized",
                "Tu sesión ya no es válida. Cierra sesión e inicia de nuevo.");
        }

        return (HttpStatusCode.BadRequest, "Validation",
            env.IsDevelopment()
                ? $"Error de integridad en base de datos: {msg}"
                : "No se pudo guardar por un dato inválido. Revisa los campos o vuelve a iniciar sesión.");
    }

    private static (HttpStatusCode Status, string Message) MapInvalidOperation(InvalidOperationException op)
    {
        if (op.Message.Contains("ADDRESS_USER_MISSING", StringComparison.OrdinalIgnoreCase))
            return (HttpStatusCode.Unauthorized, "Tu sesión ya no es válida. Cierra sesión e inicia de nuevo.");

        if (op.Message.Contains("ADDRESS_FK_VIOLATION", StringComparison.OrdinalIgnoreCase))
            return (HttpStatusCode.BadRequest, op.Message.Replace("ADDRESS_FK_VIOLATION: ", string.Empty));

        if (op.Message.Contains("ADDRESS_DB_ERROR", StringComparison.OrdinalIgnoreCase))
            return (HttpStatusCode.BadRequest, op.Message.Replace("ADDRESS_DB_ERROR: ", string.Empty));

        return (HttpStatusCode.BadRequest, op.Message);
    }

    private static string GetSqlErrorText(SqlException sql)
    {
        var parts = sql.Errors.Cast<SqlError>().Select(e => e.Message);
        return string.Join(" ", parts.Append(sql.Message));
    }

    private static bool IsAddressUserFk(string msg) =>
        msg.Contains("FK_addresses_users", StringComparison.OrdinalIgnoreCase)
        || (msg.Contains("addresses", StringComparison.OrdinalIgnoreCase)
            && msg.Contains("users", StringComparison.OrdinalIgnoreCase));

    private static ApiErrorResponse Single(string message, string code, string? propertyName = null, string? detail = null)
    {
        var text = detail is null ? message : $"{message} ({detail})";
        return new ApiErrorResponse([new ApiErrorItem(text, code, propertyName)]);
    }

    private static bool IsSchemaMismatch(Exception ex) =>
        ex switch
        {
            SqlException { Number: 207 } => true,
            DbUpdateException { InnerException: SqlException { Number: 207 } } => true,
            _ when ex.Message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) => true,
            { InnerException: not null } => IsSchemaMismatch(ex.InnerException),
            _ => false
        };

    private static string? ToCamelCase(string? name) =>
        string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];

    public static Task WriteAsync(HttpContext context, Exception ex, IHostEnvironment env)
    {
        var (status, body) = Map(ex, env);
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}

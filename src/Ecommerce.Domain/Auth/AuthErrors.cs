// Errores de negocio de autenticación (usados en handlers → FluentResults → HTTP).
using FluentResults;

namespace Ecommerce.Domain.Auth;

public static class AuthErrors
{
    public const string UnauthorizedCode = "Unauthorized";
    public const string ConflictCode = "Conflict";
    public const string NotFoundCode = "NotFound";

    public static Error InvalidCredentials() =>
        new Error("Credenciales inválidas").WithMetadata("Code", UnauthorizedCode);

    public static Error EmailAlreadyRegistered() =>
        new Error("El email ya está registrado").WithMetadata("Code", ConflictCode);

    public static Error UserNotFound() =>
        new Error("Usuario no encontrado").WithMetadata("Code", NotFoundCode);

    public static Error RoleNotConfigured(string roleCode) =>
        new Error($"Rol '{roleCode}' no configurado en el sistema").WithMetadata("Code", "Validation");
}

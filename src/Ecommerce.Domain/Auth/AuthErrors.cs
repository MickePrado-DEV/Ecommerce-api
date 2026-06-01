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

    /// <summary>JWT válido pero el usuario ya no existe (p. ej. BD recreada).</summary>
    public static Error SessionInvalid() =>
        new Error("Tu sesión ya no es válida. Cierra sesión e inicia de nuevo.")
            .WithMetadata("Code", UnauthorizedCode);

    public static Error RoleNotConfigured(string roleCode) =>
        new Error($"Rol '{roleCode}' no configurado en el sistema").WithMetadata("Code", "Validation");

    public static Error InvalidCurrentPassword() =>
        new Error("La contraseña actual no es correcta").WithMetadata("Code", UnauthorizedCode);

    public static Error InvalidState(string message) =>
        new Error(message).WithMetadata("Code", "Validation");
}

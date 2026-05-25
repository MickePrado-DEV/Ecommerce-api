using FluentResults;

namespace Ecommerce.Domain.Driver;

public static class DriverErrors
{
    public const string ForbiddenCode = "Forbidden";

    public static Error NotDriver() =>
        new Error("La cuenta no es de repartidor").WithMetadata("Code", ForbiddenCode);

    public static Error ProfileNotFound() =>
        new Error("Perfil de repartidor no encontrado").WithMetadata("Code", "NotFound");
}

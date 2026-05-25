using FluentResults;

namespace Ecommerce.Domain.Admin;

public static class AdminErrors
{
    public const string NotFoundCode = "NotFound";
    public const string ValidationCode = "Validation";

    public static Error NotFound(string entity, Guid id) =>
        new Error($"{entity} {id} no encontrado").WithMetadata("Code", NotFoundCode);

    public static Error InvalidState(string message) =>
        new Error(message).WithMetadata("Code", ValidationCode);
}

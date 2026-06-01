using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Authorization;
using FluentResults;

namespace Ecommerce.Application.Features.Admin;

internal static class AdminUserRoleValidation
{
    internal static Task<Result> ValidateRoleCodesAsync(IReadOnlyList<string> roleCodes, CancellationToken ct)
    {
        if (roleCodes.Count == 0)
            return Task.FromResult(Result.Fail(AdminErrors.Validation("Asigna al menos un rol.")));

        var validCodes = new HashSet<string>(RoleCodes.All, StringComparer.Ordinal);
        var invalid = roleCodes.Where(c => !validCodes.Contains(c)).Distinct().ToList();
        if (invalid.Count > 0)
            return Task.FromResult(Result.Fail(AdminErrors.Validation($"Roles inválidos: {string.Join(", ", invalid)}")));

        return Task.FromResult(Result.Ok());
    }
}

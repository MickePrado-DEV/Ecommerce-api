using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Auth;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Entities;
using FluentResults;

namespace Ecommerce.Application.Features.Admin;

internal static class DriverAccountProvisioning
{
    public static async Task<Result<string>> EnsureLoginAccountAsync(
        Driver driver,
        string loginEmail,
        IUserRepository users,
        IShipmentRepository shipments,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(loginEmail))
            return Result.Fail<string>(AdminErrors.InvalidState("El email es obligatorio para crear acceso de repartidor."));

        loginEmail = loginEmail.Trim();
        var temporaryPassword = DriverTemporaryPasswordGenerator.Generate();

        var (firstName, lastName) = SplitName(driver.Name);
        var roleId = await users.GetRoleIdByCodeAsync(RoleCodes.Driver, ct);
        if (roleId is null)
            return Result.Fail<string>(AuthErrors.RoleNotConfigured(RoleCodes.Driver));

        if (driver.UserId is { } linkedUserId)
        {
            var linked = await users.GetByIdAsync(linkedUserId, ct);
            if (linked is null)
            {
                driver.UserId = null;
            }
            else
            {
                if (!string.Equals(linked.Email, loginEmail, StringComparison.OrdinalIgnoreCase)
                    && await users.EmailExistsAsync(loginEmail, ct))
                {
                    return Result.Fail<string>(AuthErrors.EmailAlreadyRegistered());
                }

                if (!string.Equals(linked.Email, loginEmail, StringComparison.OrdinalIgnoreCase))
                    await users.UpdateEmailAsync(linkedUserId, loginEmail, ct);

                await users.SetTemporaryPasswordAsync(linkedUserId, temporaryPassword, ct);
                await users.AssignRoleAsync(linkedUserId, roleId.Value, ct);
                driver.UserId = linkedUserId;
                await shipments.SaveDriverAsync(driver, ct);
                return Result.Ok(temporaryPassword);
            }
        }

        if (await users.EmailExistsAsync(loginEmail, ct))
        {
            return Result.Fail<string>(AuthErrors.EmailAlreadyRegistered());
        }

        var user = new User
        {
            Email = loginEmail,
            FirstName = firstName,
            LastName = lastName,
            Phone = driver.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
            TemporaryPasswordPlain = temporaryPassword,
            MustChangePassword = true,
            IsActive = true,
        };

        await users.CreateAsync(user, ct);
        await users.AssignRoleAsync(user.Id, roleId.Value, ct);

        driver.UserId = user.Id;
        await shipments.SaveDriverAsync(driver, ct);
        return Result.Ok(temporaryPassword);
    }

    private static (string FirstName, string LastName) SplitName(string name)
    {
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return ("Repartidor", "");
        if (parts.Length == 1) return (parts[0], "");
        return (parts[0], string.Join(' ', parts.Skip(1)));
    }
}

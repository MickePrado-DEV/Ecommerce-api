using Ecommerce.Application.DTOs.Shipments;

namespace Ecommerce.Application.Features.Admin;

internal static class AdminDriverMapping
{
    public static DriverDto Map(Domain.Entities.Driver d, string? generatedTemporaryPassword = null) => new(
        d.Id,
        d.Name,
        d.Phone,
        d.Email,
        d.LicenseNumber,
        d.VehicleType,
        d.VehiclePlate,
        d.Notes,
        d.IsActive,
        d.UserId,
        d.User?.Email ?? d.Email,
        d.UserId.HasValue,
        d.User?.MustChangePassword ?? false,
        generatedTemporaryPassword);
}

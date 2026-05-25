namespace Ecommerce.Application.DTOs.Auth;

public record RegisterDriverRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string Phone,
    string? LicenseNumber = null,
    string? VehiclePlate = null);

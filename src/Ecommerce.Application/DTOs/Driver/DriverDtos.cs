namespace Ecommerce.Application.DTOs.Driver;

public record DriverProfileDto(
    Guid UserId,
    Guid DriverId,
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    string? LicenseNumber,
    string? VehiclePlate);

public record DriverShipmentDto(
    Guid ShipmentId,
    Guid OrderId,
    string OrderNumber,
    string Status,
    string? TrackingNumber,
    DateTime CreatedAt,
    string CustomerName,
    string? CustomerPhone,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country);

namespace Ecommerce.Application.DTOs.Shipments;

public record CreateShipmentRequest(Guid OrderId, Guid DriverId, string? TrackingNumber);
public record ShipmentDto(Guid Id, Guid OrderId, string Status, string? TrackingNumber, Guid? DriverId, string? TicketNumber);
public record DriverDto(Guid Id, string Name, string Phone, bool IsActive);
public record SaveDriverRequest(Guid? Id, string Name, string Phone, bool IsActive);

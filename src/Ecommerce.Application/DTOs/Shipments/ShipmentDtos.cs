namespace Ecommerce.Application.DTOs.Shipments;

public record CreateShipmentRequest(Guid OrderId, Guid DriverId, string? TrackingNumber);
public record ShipmentDto(Guid Id, Guid OrderId, string Status, string? TrackingNumber, Guid? DriverId, string? TicketNumber);
public record ShipmentSummaryDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    string Status,
    string? TrackingNumber,
    string? DriverName,
    DateTime CreatedAt);
public record DriverDto(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? LicenseNumber,
    string? VehicleType,
    string? VehiclePlate,
    string? Notes,
    bool IsActive,
    Guid? UserId = null,
    string? LoginEmail = null,
    bool HasLoginAccount = false,
    bool MustChangePassword = false,
    string? GeneratedTemporaryPassword = null);

public record PagedShipmentsAdminDto(
    IReadOnlyList<ShipmentSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

public record SaveDriverRequest(
    Guid? Id,
    string Name,
    string Phone,
    string? Email,
    string? LicenseNumber,
    string? VehicleType,
    string? VehiclePlate,
    string? Notes,
    bool IsActive,
    /// <summary>En edición: regenera usuario/contraseña temporal (generada en servidor).</summary>
    bool CreateLoginAccount = false);

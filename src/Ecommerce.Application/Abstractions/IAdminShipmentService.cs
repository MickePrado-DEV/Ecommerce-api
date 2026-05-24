using Ecommerce.Application.DTOs.Shipments;

namespace Ecommerce.Application.Abstractions;

public interface IAdminShipmentService
{
    Task<ShipmentDto> CreateShipmentAsync(CreateShipmentRequest request, CancellationToken ct = default);
    Task<byte[]> GenerateTicketPdfAsync(Guid shipmentId, CancellationToken ct = default);
    Task<IReadOnlyList<DriverDto>> ListDriversAsync(CancellationToken ct = default);
    Task<DriverDto> SaveDriverAsync(SaveDriverRequest request, CancellationToken ct = default);
    Task DeleteDriverAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ShipmentSummaryDto>> ListShipmentsAsync(int page, int pageSize, CancellationToken ct = default);
    Task MarkInTransitAsync(Guid shipmentId, CancellationToken ct = default);
    Task MarkDeliveredAsync(Guid shipmentId, CancellationToken ct = default);
}

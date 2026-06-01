using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IShipmentRepository
{
    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Shipment> CreateAsync(Shipment shipment, DispatchTicket ticket, CancellationToken ct = default);
    Task<List<Driver>> ListDriversAsync(CancellationToken ct = default);
    Task<List<Driver>> ListAllDriversAdminAsync(CancellationToken ct = default);
    Task<Driver?> GetDriverWithUserAsync(Guid id, CancellationToken ct = default);
    Task<Driver> SaveDriverAsync(Driver driver, CancellationToken ct = default);
    Task DeleteDriverAsync(Guid id, CancellationToken ct = default);
    Task<(List<Shipment> Items, int Total)> ListAsync(int page, int pageSize, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid shipmentId, ShipmentStatus status, CancellationToken ct = default);
    Task<List<Shipment>> ListByDriverIdAsync(Guid driverId, int page, int pageSize, CancellationToken ct = default);
    Task<Shipment?> GetByIdForDriverAsync(Guid shipmentId, Guid driverId, CancellationToken ct = default);
}

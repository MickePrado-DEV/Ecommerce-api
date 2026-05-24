using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IShipmentRepository
{
    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Shipment> CreateAsync(Shipment shipment, DispatchTicket ticket, CancellationToken ct = default);
    Task<List<Driver>> ListDriversAsync(CancellationToken ct = default);
    Task<Driver> SaveDriverAsync(Driver driver, CancellationToken ct = default);
}

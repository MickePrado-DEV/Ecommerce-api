using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class ShipmentRepository(EcommerceDbContext db) : IShipmentRepository
{
    public Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        db.Shipments.Include(s => s.Driver).Include(s => s.Ticket)
            .FirstOrDefaultAsync(s => s.OrderId == orderId, ct);

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Shipments.Include(s => s.Driver).Include(s => s.Order).ThenInclude(o => o.Items)
            .Include(s => s.Ticket)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Shipment> CreateAsync(Shipment shipment, DispatchTicket ticket, CancellationToken ct = default)
    {
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync(ct);
        ticket.ShipmentId = shipment.Id;
        db.DispatchTickets.Add(ticket);
        await db.SaveChangesAsync(ct);
        return shipment;
    }

    public Task<List<Driver>> ListDriversAsync(CancellationToken ct = default) =>
        db.Drivers.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(ct);

    public async Task<Driver> SaveDriverAsync(Driver driver, CancellationToken ct = default)
    {
        if (driver.Id == Guid.Empty) db.Drivers.Add(driver);
        else db.Drivers.Update(driver);
        await db.SaveChangesAsync(ct);
        return driver;
    }

    public async Task DeleteDriverAsync(Guid id, CancellationToken ct = default)
    {
        var driver = await db.Drivers.FindAsync([id], ct);
        if (driver is not null) db.Drivers.Remove(driver);
        await db.SaveChangesAsync(ct);
    }

    public Task<List<Shipment>> ListAsync(int page, int pageSize, CancellationToken ct = default) =>
        db.Shipments.AsNoTracking().Include(s => s.Driver).Include(s => s.Order)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

    public async Task UpdateStatusAsync(Guid shipmentId, ShipmentStatus status, CancellationToken ct = default)
    {
        var shipment = await db.Shipments.Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.Id == shipmentId, ct)
            ?? throw new InvalidOperationException("Envío no encontrado");
        shipment.Status = status;
        if (status == ShipmentStatus.InTransit)
            shipment.ShippedAt ??= DateTime.UtcNow;
        if (status == ShipmentStatus.Delivered && shipment.Order is not null)
            shipment.Order.Status = OrderStatus.Delivered;
        await db.SaveChangesAsync(ct);
    }

    public Task<List<Shipment>> ListByDriverIdAsync(Guid driverId, int page, int pageSize, CancellationToken ct = default) =>
        db.Shipments.AsNoTracking()
            .Include(s => s.Order).ThenInclude(o => o.Address)
            .Where(s => s.DriverId == driverId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

    public Task<Shipment?> GetByIdForDriverAsync(Guid shipmentId, Guid driverId, CancellationToken ct = default) =>
        db.Shipments
            .Include(s => s.Order).ThenInclude(o => o.Address)
            .Include(s => s.Ticket)
            .FirstOrDefaultAsync(s => s.Id == shipmentId && s.DriverId == driverId, ct);
}

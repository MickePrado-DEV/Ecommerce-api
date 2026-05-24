using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

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
}

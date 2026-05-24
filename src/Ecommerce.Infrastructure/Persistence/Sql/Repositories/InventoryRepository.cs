using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class InventoryRepository(EcommerceDbContext db) : IInventoryRepository
{
    public Task<Inventory?> GetByVariantIdAsync(Guid variantId, CancellationToken ct = default) =>
        db.Inventories.Include(i => i.Variant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(i => i.VariantId == variantId, ct);

    public async Task ReserveAsync(Guid orderId, IEnumerable<(Guid VariantId, int Quantity)> items, DateTime expiresAt, CancellationToken ct = default)
    {
        foreach (var (variantId, quantity) in items)
        {
            var inv = await db.Inventories.FirstOrDefaultAsync(i => i.VariantId == variantId, ct)
                ?? throw new NotFoundException("Inventory", variantId);

            if (inv.QuantityOnHand - inv.QuantityReserved < quantity)
                throw new InsufficientStockException(variantId);

            inv.QuantityReserved += quantity;
            db.StockReservations.Add(new StockReservation
            {
                OrderId = orderId,
                VariantId = variantId,
                Quantity = quantity,
                ExpiresAt = expiresAt
            });
            db.StockMovements.Add(new StockMovement
            {
                VariantId = variantId,
                Type = StockMovementType.Reservation,
                Quantity = quantity,
                Reference = orderId.ToString()
            });
        }
    }

    public async Task CommitReservationAsync(Guid orderId, CancellationToken ct = default)
    {
        var reservations = await db.StockReservations.Where(r => r.OrderId == orderId).ToListAsync(ct);
        foreach (var res in reservations)
        {
            var inv = await db.Inventories.FirstAsync(i => i.VariantId == res.VariantId, ct);
            inv.QuantityReserved -= res.Quantity;
            inv.QuantityOnHand -= res.Quantity;
            db.StockMovements.Add(new StockMovement
            {
                VariantId = res.VariantId,
                Type = StockMovementType.Sale,
                Quantity = -res.Quantity,
                Reference = orderId.ToString()
            });
        }
        db.StockReservations.RemoveRange(reservations);
    }

    public async Task ReleaseReservationAsync(Guid orderId, CancellationToken ct = default)
    {
        var reservations = await db.StockReservations.Where(r => r.OrderId == orderId).ToListAsync(ct);
        foreach (var res in reservations)
        {
            var inv = await db.Inventories.FirstAsync(i => i.VariantId == res.VariantId, ct);
            inv.QuantityReserved -= res.Quantity;
            db.StockMovements.Add(new StockMovement
            {
                VariantId = res.VariantId,
                Type = StockMovementType.Return,
                Quantity = res.Quantity,
                Reference = orderId.ToString()
            });
        }
        db.StockReservations.RemoveRange(reservations);
    }

    public async Task AdjustAsync(Guid variantId, int deltaOnHand, int deltaReserved, StockMovementType type, string? reference, CancellationToken ct = default)
    {
        var inv = await db.Inventories.FirstOrDefaultAsync(i => i.VariantId == variantId, ct)
            ?? throw new NotFoundException("Inventory", variantId);
        inv.QuantityOnHand += deltaOnHand;
        inv.QuantityReserved += deltaReserved;
        db.StockMovements.Add(new StockMovement
        {
            VariantId = variantId,
            Type = type,
            Quantity = deltaOnHand != 0 ? deltaOnHand : deltaReserved,
            Reference = reference
        });
        await db.SaveChangesAsync(ct);
    }

    public Task<List<Inventory>> ListAsync(CancellationToken ct = default) =>
        db.Inventories.Include(i => i.Variant).ThenInclude(v => v.Product).ToListAsync(ct);

    public async Task UpsertAsync(Guid variantId, int quantityOnHand, CancellationToken ct = default)
    {
        var inv = await db.Inventories.FindAsync([variantId], ct);
        if (inv is null)
        {
            db.Inventories.Add(new Inventory { VariantId = variantId, QuantityOnHand = quantityOnHand });
        }
        else
        {
            inv.QuantityOnHand = quantityOnHand;
        }
        await db.SaveChangesAsync(ct);
    }
}

using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

internal static class InventoryReservationQueries
{
    public static async Task ReserveAsync(
        EcommerceDbContext db,
        Guid orderId,
        IEnumerable<(Guid VariantId, int Quantity)> items,
        DateTime expiresAt,
        CancellationToken ct = default)
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
                ExpiresAt = expiresAt,
            });
            db.StockMovements.Add(new StockMovement
            {
                VariantId = variantId,
                Type = StockMovementType.Reservation,
                Quantity = quantity,
                Reference = orderId.ToString(),
            });
        }
    }

    public static async Task CommitReservationAsync(EcommerceDbContext db, Guid orderId, CancellationToken ct = default)
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
                Reference = orderId.ToString(),
            });
        }
        db.StockReservations.RemoveRange(reservations);
    }

    public static async Task ReleaseReservationAsync(EcommerceDbContext db, Guid orderId, CancellationToken ct = default)
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
                Reference = orderId.ToString(),
            });
        }
        db.StockReservations.RemoveRange(reservations);
    }
}

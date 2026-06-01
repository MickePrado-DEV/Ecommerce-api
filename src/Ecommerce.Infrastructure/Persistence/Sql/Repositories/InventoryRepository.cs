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

    public Task ReserveAsync(Guid orderId, IEnumerable<(Guid VariantId, int Quantity)> items, DateTime expiresAt, CancellationToken ct = default) =>
        InventoryReservationQueries.ReserveAsync(db, orderId, items, expiresAt, ct);

    public Task CommitReservationAsync(Guid orderId, CancellationToken ct = default) =>
        InventoryReservationQueries.CommitReservationAsync(db, orderId, ct);

    public Task ReleaseReservationAsync(Guid orderId, CancellationToken ct = default) =>
        InventoryReservationQueries.ReleaseReservationAsync(db, orderId, ct);

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
            Reference = reference,
        });
        await db.SaveChangesAsync(ct);
    }

    public Task<(List<Inventory> Items, int Total)> ListPagedAsync(
        int page, int pageSize, string? search, string? sortBy, string sortDirection, CancellationToken ct = default) =>
        InventoryListQueries.ListPagedAsync(db, page, pageSize, search, sortBy, sortDirection, ct);

    public async Task UpsertAsync(Guid variantId, int quantityOnHand, CancellationToken ct = default)
    {
        var inv = await db.Inventories.FindAsync([variantId], ct);
        if (inv is null)
            db.Inventories.Add(new Inventory { VariantId = variantId, QuantityOnHand = quantityOnHand });
        else
            inv.QuantityOnHand = quantityOnHand;
        await db.SaveChangesAsync(ct);
    }
}

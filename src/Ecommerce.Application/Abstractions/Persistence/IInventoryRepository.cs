using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Application.Abstractions.Persistence;

public interface IInventoryRepository
{
    Task<Inventory?> GetByVariantIdAsync(Guid variantId, CancellationToken ct = default);
    Task ReserveAsync(Guid orderId, IEnumerable<(Guid VariantId, int Quantity)> items, DateTime expiresAt, CancellationToken ct = default);
    Task CommitReservationAsync(Guid orderId, CancellationToken ct = default);
    Task ReleaseReservationAsync(Guid orderId, CancellationToken ct = default);
    Task AdjustAsync(Guid variantId, int deltaOnHand, int deltaReserved, StockMovementType type, string? reference, CancellationToken ct = default);
    Task<(List<Inventory> Items, int Total)> ListPagedAsync(
        int page, int pageSize, string? search, string? sortBy, string sortDirection, CancellationToken ct = default);
    Task UpsertAsync(Guid variantId, int quantityOnHand, CancellationToken ct = default);
}

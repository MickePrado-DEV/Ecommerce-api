using Ecommerce.Application.DTOs.Inventory;

namespace Ecommerce.Application.Abstractions;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryDto>> ListAsync(CancellationToken ct = default);
    Task<InventoryDto> SetStockAsync(Guid variantId, SetInventoryRequest request, CancellationToken ct = default);
}

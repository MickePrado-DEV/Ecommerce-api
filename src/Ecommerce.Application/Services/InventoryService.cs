using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Application.Services;

public class InventoryService(IInventoryRepository inventory) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryDto>> ListAsync(CancellationToken ct = default)
    {
        var items = await inventory.ListAsync(ct);
        return items.Select(i => new InventoryDto(
            i.VariantId, i.Variant.Sku, i.Variant.Product.Name,
            i.QuantityOnHand, i.QuantityReserved,
            i.QuantityOnHand - i.QuantityReserved)).ToList();
    }

    public async Task<InventoryDto> SetStockAsync(Guid variantId, SetInventoryRequest request, CancellationToken ct = default)
    {
        await inventory.UpsertAsync(variantId, request.QuantityOnHand, ct);
        var inv = await inventory.GetByVariantIdAsync(variantId, ct)
            ?? throw new Domain.Exceptions.NotFoundException("Inventory", variantId);
        return new InventoryDto(variantId, inv.Variant.Sku, inv.Variant.Product.Name,
            inv.QuantityOnHand, inv.QuantityReserved, inv.QuantityOnHand - inv.QuantityReserved);
    }
}

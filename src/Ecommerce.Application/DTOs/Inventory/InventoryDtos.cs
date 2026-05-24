namespace Ecommerce.Application.DTOs.Inventory;

public record InventoryDto(Guid VariantId, string Sku, string ProductName, int QuantityOnHand, int QuantityReserved, int Available);
public record SetInventoryRequest(int QuantityOnHand);

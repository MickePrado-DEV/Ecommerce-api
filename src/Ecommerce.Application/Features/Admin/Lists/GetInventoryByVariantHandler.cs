using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Application.DTOs.Inventory;
using Ecommerce.Domain.Admin;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record GetInventoryByVariantQuery(Guid VariantId) : IRequest<Result<InventoryDto>>;

public class GetInventoryByVariantQueryHandler(IInventoryRepository inventory)
    : IRequestHandler<GetInventoryByVariantQuery, Result<InventoryDto>>
{
    public async Task<Result<InventoryDto>> Handle(GetInventoryByVariantQuery request, CancellationToken ct)
    {
        var inv = await inventory.GetByVariantIdAsync(request.VariantId, ct);
        if (inv is null)
            return Result.Fail<InventoryDto>(AdminErrors.NotFound("Inventory", request.VariantId));

        return Result.Ok(new InventoryDto(
            inv.VariantId, inv.Variant.Sku, inv.Variant.Product.Name,
            inv.QuantityOnHand, inv.QuantityReserved,
            inv.QuantityOnHand - inv.QuantityReserved));
    }
}

using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Cart;

internal static class CartMapping
{
    public static CartDto ToDto(Domain.Entities.Cart cart)
    {
        var items = cart.Items.Select(i =>
        {
            var price = i.Variant.Price ?? i.Variant.Product.BasePrice;
            return new CartItemDto(i.Id, i.VariantId, i.Variant.Product.Name, i.Variant.Sku, i.Quantity, price, price * i.Quantity);
        }).ToList();
        return new CartDto(cart.Id, cart.GuestToken, items, items.Sum(x => x.LineTotal));
    }
}

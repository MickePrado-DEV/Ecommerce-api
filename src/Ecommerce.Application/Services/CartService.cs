using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class CartService(ICartRepository carts, IUnitOfWork uow) : ICartService
{
    public async Task<CartDto> GetAsync(Guid? userId, Guid? guestToken, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, guestToken, ct);
        return Map(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid? userId, Guid? guestToken, AddCartItemRequest request, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, guestToken, ct);
        var variant = await carts.GetVariantAsync(request.VariantId, ct)
            ?? throw new NotFoundException("Variant", request.VariantId);

        var existing = cart.Items.FirstOrDefault(i => i.VariantId == request.VariantId);
        if (existing is not null)
            existing.Quantity += request.Quantity;
        else
            cart.Items.Add(new CartItem { CartId = cart.Id, VariantId = variant.Id, Quantity = request.Quantity });

        await uow.SaveChangesAsync(ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Map(cart);
    }

    public async Task<CartDto> UpdateItemAsync(Guid? userId, Guid? guestToken, Guid itemId, UpdateCartItemRequest request, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, guestToken, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("CartItem", itemId);
        item.Quantity = request.Quantity;
        await uow.SaveChangesAsync(ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Map(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid? userId, Guid? guestToken, Guid itemId, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, guestToken, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new NotFoundException("CartItem", itemId);
        cart.Items.Remove(item);
        await uow.SaveChangesAsync(ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Map(cart);
    }

    public async Task<CartDto> MergeAsync(Guid userId, MergeCartRequest request, CancellationToken ct = default)
    {
        await carts.MergeGuestIntoUserAsync(userId, request.GuestToken, ct);
        var cart = await carts.GetOrCreateAsync(userId, null, ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Map(cart);
    }

    public async Task ClearAsync(Guid? userId, Guid? guestToken, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, guestToken, ct);
        await carts.ClearAsync(cart.Id, ct);
        await uow.SaveChangesAsync(ct);
    }

    private static CartDto Map(Cart cart)
    {
        var items = cart.Items.Select(i =>
        {
            var price = i.Variant.Price ?? i.Variant.Product.BasePrice;
            return new CartItemDto(i.Id, i.VariantId, i.Variant.Product.Name, i.Variant.Sku, i.Quantity, price, price * i.Quantity);
        }).ToList();
        return new CartDto(cart.Id, cart.GuestToken, items, items.Sum(x => x.LineTotal));
    }
}

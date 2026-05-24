using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Sql.Repositories;

public class CartRepository(EcommerceDbContext db) : ICartRepository
{
    public async Task<Cart> GetOrCreateAsync(Guid? userId, Guid? guestToken, CancellationToken ct = default)
    {
        Cart? cart = null;
        if (userId.HasValue)
            cart = await db.Carts.Include(c => c.Items).ThenInclude(i => i.Variant).ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart is null && guestToken.HasValue)
            cart = await db.Carts.Include(c => c.Items).ThenInclude(i => i.Variant).ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.GuestToken == guestToken, ct);

        if (cart is not null) return cart;

        cart = new Cart { UserId = userId, GuestToken = guestToken ?? Guid.NewGuid() };
        db.Carts.Add(cart);
        await db.SaveChangesAsync(ct);
        return cart;
    }

    public Task<Cart?> GetWithItemsAsync(Guid cartId, CancellationToken ct = default) =>
        db.Carts.Include(c => c.Items).ThenInclude(i => i.Variant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId, ct);

    public Task<Variant?> GetVariantAsync(Guid variantId, CancellationToken ct = default) =>
        db.Variants.Include(v => v.Product).Include(v => v.Inventory)
            .FirstOrDefaultAsync(v => v.Id == variantId && v.IsActive, ct);

    public async Task ClearAsync(Guid cartId, CancellationToken ct = default)
    {
        var items = await db.CartItems.Where(i => i.CartId == cartId).ToListAsync(ct);
        db.CartItems.RemoveRange(items);
    }

    public async Task MergeGuestIntoUserAsync(Guid userId, Guid guestToken, CancellationToken ct = default)
    {
        var guestCart = await db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.GuestToken == guestToken && c.UserId == null, ct);
        if (guestCart is null || guestCart.Items.Count == 0) return;

        var userCart = await db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (userCart is null)
        {
            guestCart.UserId = userId;
            guestCart.GuestToken = null;
            await db.SaveChangesAsync(ct);
            return;
        }

        foreach (var item in guestCart.Items)
        {
            var existing = userCart.Items.FirstOrDefault(i => i.VariantId == item.VariantId);
            if (existing is not null)
                existing.Quantity += item.Quantity;
            else
                userCart.Items.Add(new CartItem { CartId = userCart.Id, VariantId = item.VariantId, Quantity = item.Quantity });
        }

        db.Carts.Remove(guestCart);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteCartAsync(Guid cartId, CancellationToken ct = default)
    {
        var cart = await db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == cartId, ct);
        if (cart is null) return;
        db.CartItems.RemoveRange(cart.Items);
        db.Carts.Remove(cart);
        await db.SaveChangesAsync(ct);
    }
}

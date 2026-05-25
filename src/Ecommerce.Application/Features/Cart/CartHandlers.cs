// Carrito: lectura y commands (add/update/remove/clear/merge). Soporta userId o guestToken.
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Cart;
using Ecommerce.Domain.Cart;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Cart;

public record GetCartQuery(Guid? UserId, Guid? GuestToken) : IRequest<Result<CartDto>>;

public class GetCartQueryHandler(ICartRepository carts) : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken ct)
    {
        var cart = await carts.GetOrCreateAsync(request.UserId, request.GuestToken, ct);
        return Result.Ok(CartMapping.ToDto(cart));
    }
}

public record AddCartItemCommand(Guid? UserId, Guid? GuestToken, Guid VariantId, int Quantity) : IRequest<Result<CartDto>>;

public class AddCartItemCommandHandler(ICartRepository carts, IUnitOfWork uow)
    : IRequestHandler<AddCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
            return Result.Fail<CartDto>(CartErrors.InvalidQuantity());

        var cart = await carts.GetOrCreateAsync(request.UserId, request.GuestToken, ct);
        var variant = await carts.GetVariantAsync(request.VariantId, ct);
        if (variant is null)
            return Result.Fail<CartDto>(CartErrors.VariantNotFound(request.VariantId));

        var existing = cart.Items.FirstOrDefault(i => i.VariantId == request.VariantId);
        if (existing is not null)
            existing.Quantity += request.Quantity;
        else
            cart.Items.Add(new CartItem { CartId = cart.Id, VariantId = variant.Id, Quantity = request.Quantity });

        await uow.SaveChangesAsync(ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Result.Ok(CartMapping.ToDto(cart));
    }
}

public record UpdateCartItemCommand(Guid? UserId, Guid? GuestToken, Guid ItemId, int Quantity) : IRequest<Result<CartDto>>;

public class UpdateCartItemCommandHandler(ICartRepository carts, IUnitOfWork uow)
    : IRequestHandler<UpdateCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(UpdateCartItemCommand request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
            return Result.Fail<CartDto>(CartErrors.InvalidQuantity());

        var cart = await carts.GetOrCreateAsync(request.UserId, request.GuestToken, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null)
            return Result.Fail<CartDto>(CartErrors.CartItemNotFound(request.ItemId));

        item.Quantity = request.Quantity;
        await uow.SaveChangesAsync(ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Result.Ok(CartMapping.ToDto(cart));
    }
}

public record RemoveCartItemCommand(Guid? UserId, Guid? GuestToken, Guid ItemId) : IRequest<Result<CartDto>>;

public class RemoveCartItemCommandHandler(ICartRepository carts, IUnitOfWork uow)
    : IRequestHandler<RemoveCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken ct)
    {
        var cart = await carts.GetOrCreateAsync(request.UserId, request.GuestToken, ct);
        var item = cart.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null)
            return Result.Fail<CartDto>(CartErrors.CartItemNotFound(request.ItemId));

        cart.Items.Remove(item);
        await uow.SaveChangesAsync(ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Result.Ok(CartMapping.ToDto(cart));
    }
}

public record ClearCartCommand(Guid? UserId, Guid? GuestToken) : IRequest<Result>;

public class ClearCartCommandHandler(ICartRepository carts, IUnitOfWork uow) : IRequestHandler<ClearCartCommand, Result>
{
    public async Task<Result> Handle(ClearCartCommand request, CancellationToken ct)
    {
        var cart = await carts.GetOrCreateAsync(request.UserId, request.GuestToken, ct);
        await carts.ClearAsync(cart.Id, ct);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

public record MergeCartCommand(Guid UserId, Guid GuestToken) : IRequest<Result<CartDto>>;

public class MergeCartCommandHandler(ICartRepository carts) : IRequestHandler<MergeCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(MergeCartCommand request, CancellationToken ct)
    {
        await carts.MergeGuestIntoUserAsync(request.UserId, request.GuestToken, ct);
        var cart = await carts.GetOrCreateAsync(request.UserId, null, ct);
        cart = (await carts.GetWithItemsAsync(cart.Id, ct))!;
        return Result.Ok(CartMapping.ToDto(cart));
    }
}

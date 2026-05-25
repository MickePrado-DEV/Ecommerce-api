using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.Common;
using Ecommerce.Application.DTOs.Wishlist;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Wishlist;

public record GetWishlistQuery(Guid UserId) : IRequest<Result<IReadOnlyList<WishlistItemDto>>>;

public class GetWishlistQueryHandler(IWishlistRepository repo)
    : IRequestHandler<GetWishlistQuery, Result<IReadOnlyList<WishlistItemDto>>>
{
    public async Task<Result<IReadOnlyList<WishlistItemDto>>> Handle(GetWishlistQuery request, CancellationToken ct) =>
        Result.Ok(await repo.ListByUserAsync(request.UserId, ct));
}

public record AddToWishlistCommand(Guid UserId, Guid ProductId) : IRequest<Result>;

public class AddToWishlistCommandHandler(IWishlistRepository repo)
    : IRequestHandler<AddToWishlistCommand, Result>
{
    public async Task<Result> Handle(AddToWishlistCommand request, CancellationToken ct)
    {
        if (!await repo.ProductExistsAsync(request.ProductId, ct))
            return Result.Fail(WishlistErrors.ProductNotFound(request.ProductId));

        if (await repo.GetAsync(request.UserId, request.ProductId, ct) is not null)
            return Result.Fail(WishlistErrors.AlreadyInWishlist(request.ProductId));

        await repo.AddAsync(new WishlistItem { UserId = request.UserId, ProductId = request.ProductId }, ct);
        return Result.Ok();
    }
}

public record RemoveFromWishlistCommand(Guid UserId, Guid ProductId) : IRequest<Result>;

public class RemoveFromWishlistCommandHandler(IWishlistRepository repo)
    : IRequestHandler<RemoveFromWishlistCommand, Result>
{
    public async Task<Result> Handle(RemoveFromWishlistCommand request, CancellationToken ct)
    {
        var item = await repo.GetAsync(request.UserId, request.ProductId, ct);
        if (item is null)
            return Result.Fail(WishlistErrors.NotInWishlist(request.ProductId));

        await repo.RemoveAsync(item, ct);
        return Result.Ok();
    }
}

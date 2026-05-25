// Pedidos del cliente: listado paginado, detalle, tracking, pago y cancelación.
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Orders;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Orders;

public record ListMyOrdersQuery(Guid UserId, int Page, int PageSize, string? Status)
    : IRequest<Result<PagedOrdersDto>>;

public class ListMyOrdersQueryHandler(IOrderReadRepository readRepo)
    : IRequestHandler<ListMyOrdersQuery, Result<PagedOrdersDto>>
{
    public async Task<Result<PagedOrdersDto>> Handle(ListMyOrdersQuery request, CancellationToken ct)
    {
        var (items, total) = await readRepo.ListSummariesByUserAsync(
            request.UserId, request.Page, request.PageSize, request.Status, ct);
        return Result.Ok(new PagedOrdersDto(items, total, request.Page, request.PageSize));
    }
}

public record GetMyOrderQuery(Guid UserId, Guid OrderId) : IRequest<Result<OrderDetailDto>>;

public class GetMyOrderQueryHandler(IOrderReadRepository readRepo)
    : IRequestHandler<GetMyOrderQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetMyOrderQuery request, CancellationToken ct)
    {
        var order = await readRepo.GetDetailForUserAsync(request.OrderId, request.UserId, ct);
        return order is null
            ? Result.Fail<OrderDetailDto>(OrderErrors.NotFound(request.OrderId))
            : Result.Ok(order);
    }
}

public record GetOrderTrackingQuery(Guid UserId, Guid OrderId) : IRequest<Result<OrderTrackingDto>>;

public class GetOrderTrackingQueryHandler(IOrderReadRepository readRepo)
    : IRequestHandler<GetOrderTrackingQuery, Result<OrderTrackingDto>>
{
    public async Task<Result<OrderTrackingDto>> Handle(GetOrderTrackingQuery request, CancellationToken ct)
    {
        var tracking = await readRepo.GetTrackingForUserAsync(request.OrderId, request.UserId, ct);
        return tracking is null
            ? Result.Fail<OrderTrackingDto>(OrderErrors.NotFound(request.OrderId))
            : Result.Ok(tracking);
    }
}

public record CancelOrderCommand(Guid UserId, Guid OrderId) : IRequest<Result>;

public class CancelOrderCommandHandler(
    IOrderRepository orders, IInventoryRepository inventory, IUnitOfWork uow)
    : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await orders.GetByIdForUserAsync(request.OrderId, request.UserId, ct);
        if (order is null)
            return Result.Fail(OrderErrors.NotFound(request.OrderId));

        if (order.Status is not OrderStatus.PendingPayment and not OrderStatus.PaymentFailed)
            return Result.Fail(OrderErrors.NotCancellable());

        await uow.BeginTransactionAsync(ct);
        try
        {
            await inventory.ReleaseReservationAsync(order.Id, ct);
            order.Status = OrderStatus.Cancelled;
            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }

        return Result.Ok();
    }
}

public record PayOrderCommand(Guid UserId, Guid OrderId) : IRequest<Result<PaymentResultDto>>;

public class PayOrderCommandHandler(IOrderRepository orders, IInventoryRepository inventory, IUnitOfWork uow)
    : IRequestHandler<PayOrderCommand, Result<PaymentResultDto>>
{
    public async Task<Result<PaymentResultDto>> Handle(PayOrderCommand request, CancellationToken ct)
    {
        var order = await orders.GetByIdForUserAsync(request.OrderId, request.UserId, ct);
        if (order is null)
            return Result.Fail<PaymentResultDto>(OrderErrors.NotFound(request.OrderId));

        if (order.Status is not OrderStatus.PendingPayment and not OrderStatus.PaymentFailed)
            return Result.Fail<PaymentResultDto>(OrderErrors.NotPayable());

        await uow.BeginTransactionAsync(ct);
        try
        {
            order.Payment!.Status = PaymentStatus.Approved;
            order.Payment.PaidAt = DateTime.UtcNow;
            order.Payment.ProviderReference = $"MOCK-{Guid.NewGuid():N}";
            order.Status = OrderStatus.Paid;
            await inventory.CommitReservationAsync(order.Id, ct);
            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }

        return Result.Ok(new PaymentResultDto(order.Id, order.Status.ToString(), order.Payment.ProviderReference));
    }
}

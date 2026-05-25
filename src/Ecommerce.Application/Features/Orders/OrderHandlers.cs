// Pedidos del cliente: listado, detalle y pago mock (PayOrderCommand).
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Orders;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Orders;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Orders;

public record ListMyOrdersQuery(Guid UserId) : IRequest<Result<IReadOnlyList<OrderSummaryDto>>>;

public class ListMyOrdersQueryHandler(IOrderReadRepository readRepo)
    : IRequestHandler<ListMyOrdersQuery, Result<IReadOnlyList<OrderSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<OrderSummaryDto>>> Handle(ListMyOrdersQuery request, CancellationToken ct) =>
        Result.Ok(await readRepo.ListSummariesByUserAsync(request.UserId, ct));
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

using Ecommerce.Application.Abstractions;
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Shipments;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Application.Services;

public class AdminShipmentService(IOrderRepository orders, IShipmentRepository shipments, IPdfTicketGenerator pdf) : IAdminShipmentService
{

    public async Task<ShipmentDto> CreateShipmentAsync(CreateShipmentRequest request, CancellationToken ct = default)
    {
        var order = await orders.GetByIdAsync(request.OrderId, ct)
            ?? throw new NotFoundException("Order", request.OrderId);

        if (order.Status != OrderStatus.ReadyToDispatch)
            throw new InvalidOperationException("La orden no está lista para despacho");

        var shipment = new Shipment
        {
            OrderId = order.Id,
            DriverId = request.DriverId,
            Status = ShipmentStatus.Pending,
            TrackingNumber = request.TrackingNumber,
            ShippedAt = DateTime.UtcNow
        };
        var ticket = new DispatchTicket { TicketNumber = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}" };
        await shipments.CreateAsync(shipment, ticket, ct);
        await orders.UpdateStatusAsync(order.Id, OrderStatus.Dispatched, ct);

        return new ShipmentDto(shipment.Id, shipment.OrderId, shipment.Status.ToString(),
            shipment.TrackingNumber, shipment.DriverId, ticket.TicketNumber);
    }

    public async Task<byte[]> GenerateTicketPdfAsync(Guid shipmentId, CancellationToken ct = default)
    {
        var shipment = await shipments.GetByIdAsync(shipmentId, ct)
            ?? throw new NotFoundException("Shipment", shipmentId);

        return pdf.GenerateDispatchTicket(shipment);
    }

    public async Task<IReadOnlyList<DriverDto>> ListDriversAsync(CancellationToken ct = default)
    {
        var drivers = await shipments.ListDriversAsync(ct);
        return drivers.Select(d => new DriverDto(d.Id, d.Name, d.Phone, d.IsActive)).ToList();
    }

    public async Task<DriverDto> SaveDriverAsync(SaveDriverRequest request, CancellationToken ct = default)
    {
        var driver = new Driver
        {
            Id = request.Id ?? Guid.Empty,
            Name = request.Name,
            Phone = request.Phone,
            IsActive = request.IsActive
        };
        var saved = await shipments.SaveDriverAsync(driver, ct);
        return new DriverDto(saved.Id, saved.Name, saved.Phone, saved.IsActive);
    }
}

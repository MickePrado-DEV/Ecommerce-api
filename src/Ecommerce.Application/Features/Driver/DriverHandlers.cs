// API repartidor (mobile): perfil y gestión de envíos asignados.
using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Driver;
using Ecommerce.Domain.Admin;
using Ecommerce.Domain.Authorization;
using Ecommerce.Domain.Driver;
using DriverEntity = Ecommerce.Domain.Entities.Driver;
using Ecommerce.Domain.Emums;
using Ecommerce.Domain.Entities;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.DriverPortal;

public record GetDriverProfileQuery(Guid UserId) : IRequest<Result<DriverProfileDto>>;

public class GetDriverProfileQueryHandler(IUserRepository users, IDriverRepository drivers)
    : IRequestHandler<GetDriverProfileQuery, Result<DriverProfileDto>>
{
    public async Task<Result<DriverProfileDto>> Handle(GetDriverProfileQuery request, CancellationToken ct)
    {
        var user = await users.GetByIdWithRolesAsync(request.UserId, ct);
        if (user is null || !user.Roles.Contains(RoleCodes.Driver))
            return Result.Fail<DriverProfileDto>(DriverErrors.NotDriver());

        var driver = await drivers.GetByUserIdAsync(request.UserId, ct);
        if (driver is null)
            return Result.Fail<DriverProfileDto>(DriverErrors.ProfileNotFound());

        return Result.Ok(new DriverProfileDto(
            user.Id, driver.Id, user.Email, user.FirstName, user.LastName,
            driver.Phone, driver.LicenseNumber, driver.VehiclePlate));
    }
}

public record ListMyShipmentsQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<Result<IReadOnlyList<DriverShipmentDto>>>;

public class ListMyShipmentsQueryHandler(IDriverRepository drivers, IShipmentRepository shipments)
    : IRequestHandler<ListMyShipmentsQuery, Result<IReadOnlyList<DriverShipmentDto>>>
{
    public async Task<Result<IReadOnlyList<DriverShipmentDto>>> Handle(ListMyShipmentsQuery request, CancellationToken ct)
    {
        var driver = await drivers.GetByUserIdAsync(request.UserId, ct);
        if (driver is null)
            return Result.Fail<IReadOnlyList<DriverShipmentDto>>(DriverErrors.ProfileNotFound());

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var list = await shipments.ListByDriverIdAsync(driver.Id, page, pageSize, ct);
        return Result.Ok((IReadOnlyList<DriverShipmentDto>)list.Select(Map).ToList());
    }

    internal static DriverShipmentDto Map(Shipment s) => new(
        s.Id,
        s.OrderId,
        s.Order.OrderNumber,
        s.Status.ToString(),
        s.TrackingNumber,
        s.CreatedAt,
        s.Order.Address?.FullName ?? "—",
        s.Order.Address?.Phone,
        s.Order.Address?.Street ?? "",
        s.Order.Address?.City ?? "",
        s.Order.Address?.State ?? "",
        s.Order.Address?.PostalCode ?? "",
        s.Order.Address?.Country ?? "");
}

public record DriverUpdateShipmentStatusCommand(Guid UserId, Guid ShipmentId, ShipmentStatus Status)
    : IRequest<Result>;

public class DriverUpdateShipmentStatusCommandHandler(IDriverRepository drivers, IShipmentRepository shipments)
    : IRequestHandler<DriverUpdateShipmentStatusCommand, Result>
{
    public async Task<Result> Handle(DriverUpdateShipmentStatusCommand request, CancellationToken ct)
    {
        var driver = await drivers.GetByUserIdAsync(request.UserId, ct);
        if (driver is null)
            return Result.Fail(DriverErrors.ProfileNotFound());

        var shipment = await shipments.GetByIdForDriverAsync(request.ShipmentId, driver.Id, ct);
        if (shipment is null)
            return Result.Fail(AdminErrors.NotFound("Shipment", request.ShipmentId));

        if (request.Status is not ShipmentStatus.InTransit and not ShipmentStatus.Delivered)
            return Result.Fail(AdminErrors.InvalidState("Estado no permitido para el repartidor"));

        await shipments.UpdateStatusAsync(request.ShipmentId, request.Status, ct);
        return Result.Ok();
    }
}

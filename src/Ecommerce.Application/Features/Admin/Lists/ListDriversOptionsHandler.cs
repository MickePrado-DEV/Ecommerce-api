using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Shipments;
using Ecommerce.Application.Features.Admin;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin.Lists;

public record ListDriversOptionsQuery : IRequest<Result<IReadOnlyList<DriverDto>>>;

public class ListDriversOptionsQueryHandler(IShipmentRepository shipments)
    : IRequestHandler<ListDriversOptionsQuery, Result<IReadOnlyList<DriverDto>>>
{
    public async Task<Result<IReadOnlyList<DriverDto>>> Handle(ListDriversOptionsQuery request, CancellationToken ct)
    {
        var drivers = await shipments.ListAllDriversAdminAsync(ct);
        return Result.Ok((IReadOnlyList<DriverDto>)drivers.Select(d => AdminDriverMapping.Map(d)).ToList());
    }
}

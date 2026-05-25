using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Queries;

public class ListAddressesQueryHandler(IAddressReadRepository readRepo)
    : IRequestHandler<ListAddressesQuery, Result<IReadOnlyList<AddressDto>>>
{
    public async Task<Result<IReadOnlyList<AddressDto>>> Handle(ListAddressesQuery request, CancellationToken ct) =>
        Result.Ok(await readRepo.ListByUserAsync(request.UserId, ct));
}

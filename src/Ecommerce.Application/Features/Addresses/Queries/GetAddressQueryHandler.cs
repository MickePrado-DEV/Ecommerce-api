using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Addresses;
using Ecommerce.Domain.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Queries;

public class GetAddressQueryHandler(IAddressReadRepository readRepo)
    : IRequestHandler<GetAddressQuery, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(GetAddressQuery request, CancellationToken ct)
    {
        var address = await readRepo.GetByIdAsync(request.AddressId, request.UserId, ct);
        return address is null
            ? Result.Fail<AddressDto>(AddressErrors.NotFound(request.AddressId))
            : Result.Ok(address);
    }
}

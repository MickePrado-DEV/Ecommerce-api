using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Addresses;
using Ecommerce.Domain.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Commands;

public class SaveAddressCommandHandler(IAddressWriteRepository writeRepo)
    : IRequestHandler<SaveAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(SaveAddressCommand request, CancellationToken ct)
    {
        if (request.Id is null || request.Id == Guid.Empty)
        {
            var create = AddressRules.Create(
                request.UserId, request.Label, request.Street, request.City,
                request.State, request.PostalCode, request.Country, request.Phone, request.IsDefault);

            if (create.IsFailed)
                return Result.Fail<AddressDto>(create.Errors);

            var added = await writeRepo.AddAsync(create.Value, ct);
            return Result.Ok(Map(added));
        }

        var existing = await writeRepo.GetTrackedAsync(request.Id.Value, request.UserId, ct);
        if (existing is null)
            return Result.Fail<AddressDto>(AddressErrors.NotFound(request.Id.Value));

        var update = AddressRules.ApplyUpdate(
            existing, request.Label, request.Street, request.City,
            request.State, request.PostalCode, request.Country, request.Phone, request.IsDefault);

        if (update.IsFailed)
            return Result.Fail<AddressDto>(update.Errors);

        await writeRepo.UpdateAsync(existing, ct);
        return Result.Ok(Map(existing));
    }

    private static AddressDto Map(Domain.Entities.Address a) => new(
        a.Id, a.Label, a.Street, a.City, a.State, a.PostalCode, a.Country, a.Phone, a.IsDefault);
}

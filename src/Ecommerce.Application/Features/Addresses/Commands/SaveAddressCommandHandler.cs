using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Addresses;
using Ecommerce.Domain.Addresses;
using Ecommerce.Domain.Auth;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Commands;

public class SaveAddressCommandHandler(
    IAddressWriteRepository writeRepo,
    IAddressReadRepository readRepo,
    IUserRepository users)
    : IRequestHandler<SaveAddressCommand, Result<AddressDto>>
{
    public async Task<Result<AddressDto>> Handle(SaveAddressCommand request, CancellationToken ct)
    {
        var user = await users.GetByIdWithRolesAsync(request.UserId, ct);
        if (user is null)
            return Result.Fail<AddressDto>(AuthErrors.SessionInvalid());

        if (request.Id is null || request.Id == Guid.Empty)
        {
            var count = await readRepo.CountByUserAsync(request.UserId, ct);
            if (count >= AddressRules.MaxPerUser)
                return Result.Fail<AddressDto>(AddressErrors.MaxLimitReached(AddressRules.MaxPerUser));

            var create = AddressRules.Create(
                request.UserId, request.Type, request.Label, request.ContactName,
                request.Street, request.ExternalNumber, request.InternalNumber,
                request.Neighborhood, request.Municipality, request.City,
                request.State, request.PostalCode, request.Country, request.Phone,
                request.References, request.DeliveryInstructions,
                request.Latitude, request.Longitude, request.IsDefault);

            if (create.IsFailed)
                return Result.Fail<AddressDto>(create.Errors);

            try
            {
                var added = await writeRepo.AddAsync(create.Value, ct);
                return Result.Ok(Map(added));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ADDRESS_USER_MISSING"))
            {
                return Result.Fail<AddressDto>(AuthErrors.SessionInvalid());
            }
            catch (InvalidOperationException ex)
            {
                return Result.Fail<AddressDto>(AddressErrors.SaveFailed(ex.Message));
            }
        }

        var existing = await writeRepo.GetTrackedAsync(request.Id.Value, request.UserId, ct);
        if (existing is null)
            return Result.Fail<AddressDto>(AddressErrors.NotFound(request.Id.Value));

        var update = AddressRules.ApplyUpdate(
            existing, request.Type, request.Label, request.ContactName,
            request.Street, request.ExternalNumber, request.InternalNumber,
            request.Neighborhood, request.Municipality, request.City,
            request.State, request.PostalCode, request.Country, request.Phone,
            request.References, request.DeliveryInstructions,
            request.Latitude, request.Longitude, request.IsDefault);

        if (update.IsFailed)
            return Result.Fail<AddressDto>(update.Errors);

        try
        {
            await writeRepo.UpdateAsync(existing, ct);
            return Result.Ok(Map(existing));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ADDRESS_USER_MISSING"))
        {
            return Result.Fail<AddressDto>(AuthErrors.SessionInvalid());
        }
        catch (InvalidOperationException ex)
        {
            return Result.Fail<AddressDto>(AddressErrors.SaveFailed(ex.Message));
        }
    }

    private static AddressDto Map(Domain.Entities.Address a) => new(
        a.Id, a.Type, a.Label, a.ContactName, a.Street, a.ExternalNumber, a.InternalNumber,
        a.Neighborhood, a.Municipality, a.City, a.State, a.PostalCode, a.Country, a.Phone,
        a.References, a.DeliveryInstructions, a.Latitude, a.Longitude, a.IsDefault);
}

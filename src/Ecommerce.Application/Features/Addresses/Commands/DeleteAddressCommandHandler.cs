using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Domain.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Commands;

public class DeleteAddressCommandHandler(IAddressWriteRepository writeRepo)
    : IRequestHandler<DeleteAddressCommand, Result>
{
    public async Task<Result> Handle(DeleteAddressCommand request, CancellationToken ct)
    {
        var existing = await writeRepo.GetTrackedAsync(request.AddressId, request.UserId, ct);
        if (existing is null)
            return Result.Fail(AddressErrors.NotFound(request.AddressId));

        await writeRepo.DeleteAsync(request.AddressId, request.UserId, ct);
        return Result.Ok();
    }
}

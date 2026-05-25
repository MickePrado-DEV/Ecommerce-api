using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Commands;

public record SetDefaultAddressCommand(Guid UserId, Guid AddressId) : IRequest<Result>;

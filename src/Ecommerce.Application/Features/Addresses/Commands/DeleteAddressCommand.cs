using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Commands;

public record DeleteAddressCommand(Guid UserId, Guid AddressId) : IRequest<Result>;

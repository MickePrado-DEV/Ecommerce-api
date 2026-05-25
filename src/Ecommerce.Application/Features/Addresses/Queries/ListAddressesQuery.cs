using Ecommerce.Application.DTOs.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Queries;

public record ListAddressesQuery(Guid UserId) : IRequest<Result<IReadOnlyList<AddressDto>>>;

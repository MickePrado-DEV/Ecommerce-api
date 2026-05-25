using Ecommerce.Application.DTOs.Addresses;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Addresses.Queries;

public record GetAddressQuery(Guid UserId, Guid AddressId) : IRequest<Result<AddressDto>>;

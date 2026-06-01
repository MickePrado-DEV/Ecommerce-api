using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Admin;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin;

public record GetUserAdminQuery(Guid Id) : IRequest<Result<UserAdminDto>>;

public class GetUserAdminQueryHandler(IAdminUserRepository repo)
    : IRequestHandler<GetUserAdminQuery, Result<UserAdminDto>>
{
    public async Task<Result<UserAdminDto>> Handle(GetUserAdminQuery request, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(request.Id, ct);
        return user is null
            ? Result.Fail<UserAdminDto>(AdminErrors.NotFound("User", request.Id))
            : Result.Ok(AdminUserMapping.MapUser(user));
    }
}

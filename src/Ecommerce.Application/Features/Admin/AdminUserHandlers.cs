using Ecommerce.Application.Abstractions.Persistence;
using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Admin;
using FluentResults;
using MediatR;

namespace Ecommerce.Application.Features.Admin;

public record ListUsersAdminQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<Result<PagedUsersAdminDto>>;

public class ListUsersAdminQueryHandler(IAdminUserRepository repo)
    : IRequestHandler<ListUsersAdminQuery, Result<PagedUsersAdminDto>>
{
    public async Task<Result<PagedUsersAdminDto>> Handle(ListUsersAdminQuery request, CancellationToken ct)
    {
        var (items, total) = await repo.ListAsync(request.Page, request.PageSize, request.Search, ct);
        var dtos = items.Select(u => new UserAdminDto(
            u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.IsActive, u.Roles, u.CreatedAt)).ToList();
        return Result.Ok(new PagedUsersAdminDto(dtos, total, request.Page, request.PageSize));
    }
}

public record GetUserAdminQuery(Guid Id) : IRequest<Result<UserAdminDto>>;

public class GetUserAdminQueryHandler(IAdminUserRepository repo)
    : IRequestHandler<GetUserAdminQuery, Result<UserAdminDto>>
{
    public async Task<Result<UserAdminDto>> Handle(GetUserAdminQuery request, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(request.Id, ct);
        return user is null
            ? Result.Fail<UserAdminDto>(AdminErrors.NotFound("User", request.Id))
            : Result.Ok(new UserAdminDto(
                user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.IsActive, user.Roles, user.CreatedAt));
    }
}

public record UpdateUserAdminCommand(Guid Id, bool IsActive, IReadOnlyList<string> RoleCodes) : IRequest<Result<UserAdminDto>>;

public class UpdateUserAdminCommandHandler(IAdminUserRepository repo)
    : IRequestHandler<UpdateUserAdminCommand, Result<UserAdminDto>>
{
    public async Task<Result<UserAdminDto>> Handle(UpdateUserAdminCommand request, CancellationToken ct)
    {
        await repo.UpdateAsync(request.Id, request.IsActive, request.RoleCodes, ct);
        var user = await repo.GetByIdAsync(request.Id, ct);
        return user is null
            ? Result.Fail<UserAdminDto>(AdminErrors.NotFound("User", request.Id))
            : Result.Ok(new UserAdminDto(
                user.Id, user.Email, user.FirstName, user.LastName, user.Phone, user.IsActive, user.Roles, user.CreatedAt));
    }
}

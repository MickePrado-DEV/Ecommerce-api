using Ecommerce.Application.DTOs.Admin;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Features.Admin;

internal static class AdminUserMapping
{
    internal static UserAdminDto MapUser(User u) => new(
        u.Id, u.Email, u.FirstName, u.LastName, u.Phone, u.IsActive, u.Roles, u.CreatedAt);
}

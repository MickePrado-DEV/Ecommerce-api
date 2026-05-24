using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

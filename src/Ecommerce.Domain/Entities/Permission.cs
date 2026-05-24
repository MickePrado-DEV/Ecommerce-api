using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

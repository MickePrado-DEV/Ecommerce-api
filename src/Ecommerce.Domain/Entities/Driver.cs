using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Driver : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

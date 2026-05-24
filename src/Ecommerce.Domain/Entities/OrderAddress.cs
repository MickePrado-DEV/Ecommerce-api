using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class OrderAddress : BaseEntity
{
    public Guid OrderId { get; set; }
    public string FullName { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public Order Order { get; set; } = null!;
}

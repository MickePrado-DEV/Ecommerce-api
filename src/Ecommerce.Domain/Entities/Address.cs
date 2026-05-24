using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public string Label { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public bool IsDefault { get; set; }
    public User User { get; set; } = null!;
}

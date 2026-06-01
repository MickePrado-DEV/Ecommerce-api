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
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? AddressText { get; set; }
    public Order Order { get; set; } = null!;
}

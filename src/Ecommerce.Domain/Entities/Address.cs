using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public int Type { get; set; } = 1;
    public string Label { get; set; } = null!;
    public string? ContactName { get; set; }
    public string Street { get; set; } = null!;
    public string? ExternalNumber { get; set; }
    public string? InternalNumber { get; set; }
    public string? Neighborhood { get; set; }
    public string? Municipality { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? References { get; set; }
    public string? DeliveryInstructions { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsDefault { get; set; }
    public User User { get; set; } = null!;
}

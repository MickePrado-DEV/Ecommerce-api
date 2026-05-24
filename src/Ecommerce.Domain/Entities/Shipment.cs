using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class Shipment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid? DriverId { get; set; }
    public ShipmentStatus Status { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedAt { get; set; }
    public Order Order { get; set; } = null!;
    public Driver? Driver { get; set; }
    public DispatchTicket? Ticket { get; set; }
}

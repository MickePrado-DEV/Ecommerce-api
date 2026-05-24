using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class DispatchTicket : BaseEntity
{
    public Guid ShipmentId { get; set; }
    public string TicketNumber { get; set; } = null!;
    public Shipment Shipment { get; set; } = null!;
}

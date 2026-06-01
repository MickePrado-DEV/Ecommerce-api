using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class DeliveryRouteStop : BaseEntity
{
    public Guid RouteId { get; set; }
    public Guid OrderId { get; set; }
    public int StopIndex { get; set; }
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
    public string AddressText { get; set; } = null!;
    public DeliveryRouteStopStatus Status { get; set; } = DeliveryRouteStopStatus.Pending;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }

    public DeliveryRoute Route { get; set; } = null!;
    public Order Order { get; set; } = null!;
}

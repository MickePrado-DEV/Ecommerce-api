using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class DeliveryRoute : BaseEntity
{
    public string Code { get; set; } = null!;
    public DeliveryRouteStatus Status { get; set; } = DeliveryRouteStatus.Draft;
    public Guid? DriverId { get; set; }
    public Guid? BatchId { get; set; }
    public DeliveryRouteOriginType OriginType { get; set; }
    public decimal? OriginLat { get; set; }
    public decimal? OriginLng { get; set; }
    public int TotalStops { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public Driver? Driver { get; set; }
    public DispatchBatch? Batch { get; set; }
    public ICollection<DeliveryRouteStop> Stops { get; set; } = [];
}

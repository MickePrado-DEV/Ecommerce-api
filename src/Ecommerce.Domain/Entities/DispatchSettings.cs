using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class DispatchSettings : BaseEntity
{
    public decimal DefaultClusterRadiusKm { get; set; } = 2.5m;
    public int DefaultMaxStopsPerRoute { get; set; } = 20;
    public int DefaultMaxStopsPerBatch { get; set; } = 20;
    public DeliveryRouteOriginType DefaultRouteOriginType { get; set; } = DeliveryRouteOriginType.Centroid;
    public bool AllowOriginSelection { get; set; } = true;
}

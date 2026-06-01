using Ecommerce.Domain.Common;
using Ecommerce.Domain.Emums;

namespace Ecommerce.Domain.Entities;

public class DispatchBatch : BaseEntity
{
    public string Code { get; set; } = null!;
    public DispatchBatchStatus Status { get; set; } = DispatchBatchStatus.Open;
    public decimal CenterLat { get; set; }
    public decimal CenterLng { get; set; }
    public decimal RadiusKm { get; set; }
    public int MaxStops { get; set; }

    public ICollection<DispatchBatchOrder> BatchOrders { get; set; } = [];
    public ICollection<DeliveryRoute> Routes { get; set; } = [];
}

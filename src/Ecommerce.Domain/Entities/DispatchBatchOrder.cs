using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class DispatchBatchOrder : BaseEntity
{
    public Guid BatchId { get; set; }
    public Guid OrderId { get; set; }
    public decimal? DistanceKm { get; set; }

    public DispatchBatch Batch { get; set; } = null!;
    public Order Order { get; set; } = null!;
}

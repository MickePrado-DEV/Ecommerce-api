using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Comment { get; set; } = null!;
    public bool IsApproved { get; set; }
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}

using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public Guid? UserId { get; set; }
        public Guid? GuestToken { get; set; }
        public ICollection<CartItem> Items { get; set; } = [];
    }
}

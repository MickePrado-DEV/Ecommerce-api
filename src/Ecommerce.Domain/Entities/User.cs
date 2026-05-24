using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<UserRole> UserRoles { get; set; } = [];
        public ICollection<Address> Addresses { get; set; } = [];
    }
}


using Ecommerce.Domain.Entities;
namespace Ecommerce.Infrastructure.Persistence.Sql

{

    public class EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Variant> Variants => Set<Variant>();
        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Variant>().ToTable("variants");
            modelBuilder.Entity<Inventory>().ToTable("inventory").HasKey(i => i.VariantId);
            modelBuilder.Entity<Cart>().ToTable("carts");
            modelBuilder.Entity<CartItem>().ToTable("cart_items");
            modelBuilder.Entity<Order>().ToTable("orders");

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(30);
        }
    }
}

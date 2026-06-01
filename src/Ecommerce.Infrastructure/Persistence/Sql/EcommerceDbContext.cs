using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Sql;

public class EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Subcategory> Subcategories => Set<Subcategory>();
    public DbSet<Cover> Covers => Set<Cover>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<OptionValue> OptionValues => Set<OptionValue>();
    public DbSet<ProductOptionAssignment> ProductOptionAssignments => Set<ProductOptionAssignment>();
    public DbSet<VariantOptionValue> VariantOptionValues => Set<VariantOptionValue>();
    public DbSet<Variant> Variants => Set<Variant>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DispatchTicket> DispatchTickets => Set<DispatchTicket>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<DispatchSettings> DispatchSettings => Set<DispatchSettings>();
    public DbSet<DispatchBatch> DispatchBatches => Set<DispatchBatch>();
    public DbSet<DispatchBatchOrder> DispatchBatchOrders => Set<DispatchBatchOrder>();
    public DbSet<DeliveryRoute> DeliveryRoutes => Set<DeliveryRoute>();
    public DbSet<DeliveryRouteStop> DeliveryRouteStops => Set<DeliveryRouteStop>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Role>().ToTable("roles");
        modelBuilder.Entity<Permission>().ToTable("permissions");
        modelBuilder.Entity<Permission>().HasIndex(p => p.Code).IsUnique();
        modelBuilder.Entity<UserRole>().ToTable("user_roles").HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<RolePermission>().ToTable("role_permissions").HasKey(x => new { x.RoleId, x.PermissionId });
        modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");
        modelBuilder.Entity<Family>().ToTable("families");
        modelBuilder.Entity<Category>().ToTable("categories");
        modelBuilder.Entity<Subcategory>().ToTable("subcategories");
        modelBuilder.Entity<Cover>().ToTable("covers");
        modelBuilder.Entity<Product>().ToTable("products");
        modelBuilder.Entity<ProductImage>().ToTable("product_images");
        modelBuilder.Entity<ProductOption>().ToTable("product_options");
        modelBuilder.Entity<OptionValue>().ToTable("option_values");
        modelBuilder.Entity<ProductOptionAssignment>().ToTable("product_option_assignments")
            .HasKey(x => new { x.ProductId, x.ProductOptionId });
        modelBuilder.Entity<ProductOptionAssignment>()
            .HasOne(a => a.Product).WithMany(p => p.OptionAssignments).HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProductOptionAssignment>()
            .HasOne(a => a.ProductOption).WithMany(o => o.ProductAssignments).HasForeignKey(a => a.ProductOptionId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<VariantOptionValue>().ToTable("variant_option_values")
            .HasKey(x => new { x.VariantId, x.OptionValueId });
        modelBuilder.Entity<VariantOptionValue>()
            .HasOne(x => x.Variant).WithMany(v => v.OptionValues).HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<VariantOptionValue>()
            .HasOne(x => x.OptionValue).WithMany().HasForeignKey(x => x.OptionValueId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Variant>().ToTable("variants");
        modelBuilder.Entity<WishlistItem>().ToTable("wishlist_items");
        modelBuilder.Entity<WishlistItem>()
            .HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
        modelBuilder.Entity<WishlistItem>()
            .HasOne(w => w.User).WithMany().HasForeignKey(w => w.UserId);
        modelBuilder.Entity<WishlistItem>()
            .HasOne(w => w.Product).WithMany().HasForeignKey(w => w.ProductId);
        modelBuilder.Entity<ProductReview>().ToTable("product_reviews");
        modelBuilder.Entity<ProductReview>()
            .HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
        modelBuilder.Entity<ProductReview>()
            .HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
        modelBuilder.Entity<Coupon>().ToTable("coupons");
        modelBuilder.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();
        modelBuilder.Entity<Coupon>().Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Inventory>().ToTable("inventory").HasKey(i => i.VariantId);
        modelBuilder.Entity<Cart>().ToTable("carts");
        modelBuilder.Entity<CartItem>().ToTable("cart_items");
        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<OrderItem>().ToTable("order_items");
        modelBuilder.Entity<OrderAddress>().ToTable("order_addresses");
        modelBuilder.Entity<Payment>().ToTable("payments");
        modelBuilder.Entity<StockReservation>().ToTable("stock_reservations");
        modelBuilder.Entity<StockMovement>().ToTable("stock_movements");
        modelBuilder.Entity<Shipment>().ToTable("shipments");
        modelBuilder.Entity<Driver>().ToTable("drivers");
        modelBuilder.Entity<Driver>()
            .HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<DispatchTicket>().ToTable("dispatch_tickets");
        modelBuilder.Entity<Address>().ToTable("addresses");
        modelBuilder.Entity<DispatchSettings>().ToTable("dispatch_settings");
        modelBuilder.Entity<DispatchBatch>().ToTable("dispatch_batches");
        modelBuilder.Entity<DispatchBatch>().HasIndex(b => b.Code).IsUnique();
        modelBuilder.Entity<DispatchBatchOrder>().ToTable("dispatch_batch_orders");
        modelBuilder.Entity<DispatchBatchOrder>().HasIndex(bo => bo.OrderId).IsUnique();
        modelBuilder.Entity<DispatchBatchOrder>()
            .HasOne(bo => bo.Batch).WithMany(b => b.BatchOrders).HasForeignKey(bo => bo.BatchId);
        modelBuilder.Entity<DispatchBatchOrder>()
            .HasOne(bo => bo.Order).WithMany().HasForeignKey(bo => bo.OrderId);
        modelBuilder.Entity<DeliveryRoute>().ToTable("delivery_routes");
        modelBuilder.Entity<DeliveryRoute>().HasIndex(r => r.Code).IsUnique();
        modelBuilder.Entity<DeliveryRoute>()
            .HasOne(r => r.Driver).WithMany().HasForeignKey(r => r.DriverId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<DeliveryRoute>()
            .HasOne(r => r.Batch).WithMany(b => b.Routes).HasForeignKey(r => r.BatchId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<DeliveryRouteStop>().ToTable("delivery_route_stops");
        modelBuilder.Entity<DeliveryRouteStop>().HasIndex(s => new { s.RouteId, s.OrderId }).IsUnique();
        modelBuilder.Entity<DeliveryRouteStop>().HasIndex(s => new { s.RouteId, s.StopIndex }).IsUnique();
        modelBuilder.Entity<DeliveryRouteStop>()
            .HasOne(s => s.Route).WithMany(r => r.Stops).HasForeignKey(s => s.RouteId);
        modelBuilder.Entity<DeliveryRouteStop>()
            .HasOne(s => s.Order).WithMany().HasForeignKey(s => s.OrderId);

        ConfigureEnumConversions(modelBuilder);
        ConfigureDecimals(modelBuilder);
        ConfigureGeoDecimals(modelBuilder);

        modelBuilder.Entity<Address>().Property(a => a.Latitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<Address>().Property(a => a.Longitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Address).WithOne(a => a.Order).HasForeignKey<OrderAddress>(a => a.OrderId);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Payment).WithOne(p => p.Order).HasForeignKey<Payment>(p => p.OrderId);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Shipment).WithOne(s => s.Order).HasForeignKey<Shipment>(s => s.OrderId);
        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.Ticket).WithOne(t => t.Shipment).HasForeignKey<DispatchTicket>(t => t.ShipmentId);

        modelBuilder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        modelBuilder.Entity<Family>().HasIndex(f => f.Slug).IsUnique();
    }

    private static void ConfigureEnumConversions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Order>().Property(o => o.DispatchStatus).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Payment>().Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Shipment>().Property(s => s.Status).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<StockMovement>().Property(s => s.Type).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<DispatchBatch>().Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<DeliveryRoute>().Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<DeliveryRoute>().Property(r => r.OriginType).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<DeliveryRouteStop>().Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<DispatchSettings>().Property(s => s.DefaultRouteOriginType).HasConversion<string>().HasMaxLength(20);
    }

    private static void ConfigureDecimals(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties().Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
                prop.SetColumnType("decimal(18,2)");
        }
    }

    private static void ConfigureGeoDecimals(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderAddress>().Property(a => a.Latitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<OrderAddress>().Property(a => a.Longitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<Driver>().Property(d => d.StartLatitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<Driver>().Property(d => d.StartLongitude).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<DispatchBatch>().Property(b => b.CenterLat).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<DispatchBatch>().Property(b => b.CenterLng).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<DispatchBatch>().Property(b => b.RadiusKm).HasColumnType("decimal(8,3)");
        modelBuilder.Entity<DispatchBatchOrder>().Property(bo => bo.DistanceKm).HasColumnType("decimal(8,3)");
        modelBuilder.Entity<DeliveryRoute>().Property(r => r.OriginLat).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<DeliveryRoute>().Property(r => r.OriginLng).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<DeliveryRoute>().Property(r => r.TotalDistanceKm).HasColumnType("decimal(10,3)");
        modelBuilder.Entity<DeliveryRouteStop>().Property(s => s.Lat).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<DeliveryRouteStop>().Property(s => s.Lng).HasColumnType("decimal(10,7)");
        modelBuilder.Entity<DispatchSettings>().Property(s => s.DefaultClusterRadiusKm).HasColumnType("decimal(8,3)");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

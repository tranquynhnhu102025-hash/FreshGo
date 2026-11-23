using FreshGo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FreshGo.Data;

public class FreshGoDbContext : IdentityDbContext<ApplicationUser>
{
    public FreshGoDbContext(DbContextOptions<FreshGoDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<StockRequest> StockRequests => Set<StockRequest>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<DefectiveProduct> DefectiveProducts => Set<DefectiveProduct>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Gift> Gifts => Set<Gift>();
    public DbSet<GiftRedemption> GiftRedemptions => Set<GiftRedemption>();
    public DbSet<CustomerComplaint> CustomerComplaints => Set<CustomerComplaint>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().HasOne<Branch>().WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Product>().Property(x => x.Price).HasPrecision(18, 2);
        builder.Entity<Product>().Property(x => x.CostPrice).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Entity<Voucher>().Property(x => x.DiscountPercent).HasPrecision(5, 2);
        builder.Entity<Voucher>().Property(x => x.MaxDiscount).HasPrecision(18, 2);
        builder.Entity<Voucher>().Property(x => x.MinOrderValue).HasPrecision(18, 2);
        builder.Entity<Inventory>().HasIndex(x => new { x.WarehouseId, x.ProductId }).IsUnique();
        builder.Entity<Warehouse>().HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Product>().HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Inventory>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Inventory>().HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Order>().HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<OrderItem>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Shipment>().HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<StockRequest>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<StockRequest>().HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<DefectiveProduct>().HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<DefectiveProduct>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Branch>().HasIndex(x => x.Name);
        builder.Entity<Product>().HasIndex(x => x.Name);
        builder.Entity<Voucher>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Order>().HasIndex(x => x.OrderCode).IsUnique();
        builder.Entity<GiftRedemption>().HasOne(x => x.Gift).WithMany().HasForeignKey(x => x.GiftId).OnDelete(DeleteBehavior.Restrict);
    }
}

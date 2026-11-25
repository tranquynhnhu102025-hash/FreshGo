using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FreshGo.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Shipper = "Shipper";
    public const string Customer = "Customer";
}

public static class OrderStates
{
    public const string Pending = "Chờ xác nhận";
    public const string Preparing = "Đang chuẩn bị";
    public const string Ready = "Chờ shipper";
    public const string Delivering = "Đang giao";
    public const string Delivered = "Đã giao";
    public const string Completed = "Hoàn tất";
    public const string Cancelled = "Đã hủy";
    public const string Failed = "Giao thất bại";
}

public static class RequestStates
{
    public const string Pending = "Chờ duyệt";
    public const string Approved = "Đã duyệt";
    public const string Rejected = "Từ chối";
    public const string Received = "Đã nhận hàng";
}

public class ApplicationUser : IdentityUser
{
    [MaxLength(120)] public string FullName { get; set; } = "";
    public int? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public int Points { get; set; }
}

public class Branch
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    [Required, MaxLength(300)] public string Address { get; set; } = "";
    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(250)] public string? MapUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Category
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
}

public class Product
{
    public int Id { get; set; }
    [Required, MaxLength(180)] public string Name { get; set; } = "";
    [MaxLength(1000)] public string? Description { get; set; }
    [MaxLength(500)] public string? ImageUrl { get; set; }
    [MaxLength(30)] public string Unit { get; set; } = "kg";
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Warehouse
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    public bool IsMain { get; set; }
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
}

public class Inventory
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public int MinQuantity { get; set; } = 5;
}

public class StockRequest
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [MaxLength(30)] public string Status { get; set; } = RequestStates.Pending;
    public string RequestedById { get; set; } = "";
    public string? ApprovedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
}

public class StockTransfer
{
    public int Id { get; set; }
    public int StockRequestId { get; set; }
    public int SourceWarehouseId { get; set; }
    public int TargetWarehouseId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Đang vận chuyển";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedAt { get; set; }
}

public class Order
{
    public int Id { get; set; }
    [Required, MaxLength(30)] public string OrderCode { get; set; } = "";
    public string CustomerId { get; set; } = "";
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(30)] public string Status { get; set; } = OrderStates.Pending;
    [MaxLength(30)] public string PaymentMethod { get; set; } = "COD";
    [MaxLength(30)] public string PaymentStatus { get; set; } = "Chưa thanh toán";
    [Required, MaxLength(500)] public string DeliveryAddress { get; set; } = "";
    [MaxLength(30)] public string? CustomerPhone { get; set; }
    [MaxLength(40)] public string? VoucherCode { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? ShipperId { get; set; }
    public DateTime? CustomerConfirmedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public string ShipperId { get; set; } = "";
    [MaxLength(30)] public string Status { get; set; } = "Đã nhận đơn";
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    [MaxLength(500)] public string? FailureReason { get; set; }
}

public class Attendance
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = "";
    public int BranchId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

public class DefectiveProduct
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = "";
    [MaxLength(500)] public string? ImageUrl { get; set; }
    public string ReportedById { get; set; } = "";
    public string? ReviewedById { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Chờ xác nhận";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Voucher
{
    public int Id { get; set; }
    [Required, MaxLength(40)] public string Code { get; set; } = "";
    [MaxLength(150)] public string Name { get; set; } = "";
    public decimal DiscountPercent { get; set; }
    public decimal MaxDiscount { get; set; }
    public decimal MinOrderValue { get; set; }
    public int Quantity { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Event
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Title { get; set; } = "";
    [MaxLength(1000)] public string? Description { get; set; }
    [MaxLength(500)] public string? ImageUrl { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LoyaltyTransaction
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int Points { get; set; }
    [MaxLength(250)] public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    [MaxLength(100)] public string Action { get; set; } = "";
    [MaxLength(1000)] public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Gift
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    [MaxLength(500)] public string? Description { get; set; }
    public int PointsRequired { get; set; }
    public int Quantity { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GiftRedemption
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int GiftId { get; set; }
    public Gift? Gift { get; set; }
    public int PointsSpent { get; set; }
    [MaxLength(40)] public string Status { get; set; } = "Chờ nhận quà";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CustomerComplaint
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int? OrderId { get; set; }
    public int? BranchId { get; set; }
    [Required, MaxLength(180)] public string Subject { get; set; } = "";
    [Required, MaxLength(2000)] public string Description { get; set; } = "";
    [MaxLength(500)] public string? ImageUrl { get; set; }
    [MaxLength(40)] public string Status { get; set; } = "Mới tiếp nhận";
    [MaxLength(1000)] public string? Reply { get; set; }
    public string? ReviewedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

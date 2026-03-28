namespace FluentSeeding.Samples.AspNetCore.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? CouponId { get; set; }
    public Coupon? Coupon { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime PlacedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> OrderItems { get; set; } = [];
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

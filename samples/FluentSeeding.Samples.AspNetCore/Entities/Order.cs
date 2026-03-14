namespace FluentSeeding.Samples.AspNetCore.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> OrderItems { get; set; } = [];
}

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

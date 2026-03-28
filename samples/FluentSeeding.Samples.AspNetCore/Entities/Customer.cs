namespace FluentSeeding.Samples.AspNetCore.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public DateOnly MemberSince { get; set; }
    public bool IsActive { get; set; }
    public List<Order> Orders { get; set; } = [];
}

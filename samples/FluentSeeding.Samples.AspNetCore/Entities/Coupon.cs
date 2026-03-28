namespace FluentSeeding.Samples.AspNetCore.Entities;

public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public DateOnly ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

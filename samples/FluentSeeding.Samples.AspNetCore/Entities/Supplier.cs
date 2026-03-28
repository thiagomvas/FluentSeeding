namespace FluentSeeding.Samples.AspNetCore.Entities;

public class Supplier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = [];
}

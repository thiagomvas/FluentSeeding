using FluentSeeding.AspNetCore;
using FluentSeeding.EntityFrameworkCore;
using FluentSeeding.Samples.AspNetCore.Data;
using FluentSeeding.Samples.AspNetCore.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddFluentSeeding(seeders => seeders.AddSeedersFromAssemblyContaining<OrderSeeder>())
    .AddFluentSeedingEntityFrameworkCore<SampleDbContext>(options =>
        options.ConflictBehavior = ConflictBehavior.Skip);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    db.Database.EnsureCreated();
}

await app.RunSeedersAsync();

// Suppliers
app.MapGet("/suppliers", async (SampleDbContext db) =>
    await db.Suppliers.AsNoTracking().IgnoreAutoIncludes().ToListAsync());

// Categories
app.MapGet("/categories", async (SampleDbContext db) =>
    await db.Categories.AsNoTracking().IgnoreAutoIncludes().ToListAsync());

// Coupons
app.MapGet("/coupons", async (SampleDbContext db) =>
    await db.Coupons.AsNoTracking().ToListAsync());

// Products with category and supplier
app.MapGet("/products", async (SampleDbContext db) =>
    await db.Products
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(p => p.Category)
        .Include(p => p.Supplier)
        .ToListAsync());

// Single product with its reviews
app.MapGet("/products/{id:guid}", async (Guid id, SampleDbContext db) =>
{
    var product = await db.Products
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(p => p.Category)
        .Include(p => p.Supplier)
        .Include(p => p.Reviews).ThenInclude(r => r.Customer)
        .FirstOrDefaultAsync(p => p.Id == id);

    return product is null ? Results.NotFound() : Results.Ok(product);
});

// Customers
app.MapGet("/customers", async (SampleDbContext db) =>
    await db.Customers.AsNoTracking().IgnoreAutoIncludes().ToListAsync());

// Single customer with their orders
app.MapGet("/customers/{id:guid}", async (Guid id, SampleDbContext db) =>
{
    var customer = await db.Customers
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(c => c.Orders).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.Product)
        .FirstOrDefaultAsync(c => c.Id == id);

    return customer is null ? Results.NotFound() : Results.Ok(customer);
});

// Orders with full detail
app.MapGet("/orders", async (SampleDbContext db) =>
    await db.Orders
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(o => o.Customer)
        .Include(o => o.Coupon)
        .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
        .ToListAsync());

// Reviews
app.MapGet("/reviews", async (SampleDbContext db) =>
    await db.ProductReviews
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(r => r.Product)
        .Include(r => r.Customer)
        .ToListAsync());

app.Run();
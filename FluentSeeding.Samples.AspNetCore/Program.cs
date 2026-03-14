using FluentSeeding;
using FluentSeeding.DependencyInjection;
using FluentSeeding.EntityFrameworkCore;
using FluentSeeding.Samples.AspNetCore.Data;
using FluentSeeding.Samples.AspNetCore.Entities;
using FluentSeeding.Samples.AspNetCore.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlite("Data Source=sample.db"));

builder.Services.AddFluentSeeding(seeders => { seeders.AddSeedersFromAssemblyContaining<OrderSeeder>(); });

builder.Services.AddFluentSeedingEntityFrameworkCore<SampleDbContext>(options =>
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

app.MapPost("/seed", async (SeederRunner runner, CancellationToken ct) =>
{
    await runner.RunAsync(ct);
    return Results.Ok("Seeding completed.");
});

app.MapGet("/categories", async (SampleDbContext db) =>
    await db.Categories.AsNoTracking().IgnoreAutoIncludes().ToListAsync());

app.MapGet("/products", async (SampleDbContext db) =>
    await db.Products.AsNoTracking().IgnoreAutoIncludes().Include(p => p.Category).ToListAsync());

app.MapGet("/customers", async (SampleDbContext db) =>
    await db.Customers.AsNoTracking().IgnoreAutoIncludes().ToListAsync());

app.MapGet("/orders", async (SampleDbContext db) =>
    await db.Orders
        .AsNoTracking()
        .IgnoreAutoIncludes()
        .Include(o => o.Customer)
        .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
        .ToListAsync());

app.Run();
using Microsoft.EntityFrameworkCore;
using VCommerce.Api.Endpoints;
using VCommerce.Application.Common.Interfaces;
using VCommerce.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add DbContext with In-Memory database for demonstration
// In production, use SQL Server or another database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("VCommerceDb"));

// Register IApplicationDbContext (Clean Architecture - Dependency Inversion)
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Register DbContext for handlers that need it
builder.Services.AddScoped<DbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

// Add MediatR (scan Application layer for handlers)
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(VCommerce.Application.Products.Commands.CreateProduct.CreateProductCommand).Assembly);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapProductsEndpoints();

app.Run();

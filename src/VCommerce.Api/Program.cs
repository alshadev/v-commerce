using Microsoft.EntityFrameworkCore;
using VCommerce.Api.Endpoints;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Modules.Products;
using VCommerce.Modules.Products.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add DbContext with In-Memory database for demonstration
// In production, use SQL Server or another database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("VCommerceDb");
    // Configure model manually for Products module
    options.ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCustomizer, ProductsModelCustomizer>();
});

// Add MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(ProductsModuleExtensions).Assembly);
});

// Add modules
builder.Services.AddProductsModule();

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

// Custom model customizer to apply module configurations
internal class ProductsModelCustomizer : Microsoft.EntityFrameworkCore.Infrastructure.ModelCustomizer
{
    public ProductsModelCustomizer(Microsoft.EntityFrameworkCore.Infrastructure.ModelCustomizerDependencies dependencies)
        : base(dependencies)
    {
    }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        
        // Apply configurations from Products module
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Product).Assembly);
    }
}

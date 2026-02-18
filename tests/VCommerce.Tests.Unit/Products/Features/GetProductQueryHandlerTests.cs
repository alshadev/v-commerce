using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Products.Queries.GetProduct;
using VCommerce.Domain.Products;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Infrastructure.Persistence.Configurations;

namespace VCommerce.Tests.Unit.Products.Features;

public class GetProductQueryHandlerTests
{
    private class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply Product configuration
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenProductExistsAndNotDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        
        // Create a product
        var product = Product.Create("Test Product", "Description", 99.99m, 10);
        context.Set<Product>().Add(product);
        await context.SaveChangesAsync();
        
        var handler = new GetProductQueryHandler(context);
        var query = new GetProductQuery(product.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductIsDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        
        // Create and delete a product
        var product = Product.Create("Test Product", "Description", 99.99m, 10);
        product.Delete();
        context.Set<Product>().Add(product);
        await context.SaveChangesAsync();
        
        var handler = new GetProductQueryHandler(context);
        var query = new GetProductQuery(product.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        
        var handler = new GetProductQueryHandler(context);
        var query = new GetProductQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}

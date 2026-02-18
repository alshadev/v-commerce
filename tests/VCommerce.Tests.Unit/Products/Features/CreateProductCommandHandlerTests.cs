using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Products.Commands.CreateProduct;
using VCommerce.Domain.Products;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Infrastructure.Persistence.Configurations;

namespace VCommerce.Tests.Unit.Products.Features;

public class CreateProductCommandHandlerTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new TestApplicationDbContext(options);
        return context;
    }

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
    public async Task Handle_ShouldCreateProduct_WithValidCommand()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new CreateProductCommandHandler(context, context);
        var command = new CreateProductCommand("Test Product", "Description", 99.99m, 10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        if (!result.IsSuccess)
        {
            throw new Exception($"Test failed with error: {result.Error}");
        }
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var product = await context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == result.Value);
        
        product.Should().NotBeNull();
        product!.Name.Should().Be("Test Product");
        product.Description.Should().Be("Description");
        product.Price.Should().Be(99.99m);
        product.Stock.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNameIsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new CreateProductCommandHandler(context, context);
        var command = new CreateProductCommand("", "Description", 99.99m, 10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Name cannot be empty");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPriceIsNegative()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new CreateProductCommandHandler(context, context);
        var command = new CreateProductCommand("Test Product", "Description", -10m, 10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Price cannot be negative");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStockIsNegative()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new CreateProductCommandHandler(context, context);
        var command = new CreateProductCommand("Test Product", "Description", 99.99m, -5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Stock cannot be negative");
    }
}

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
        var command = new CreateProductCommand("PROD001", "Test Product", "Description", 99.99m, 10);

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
        product!.Code.Should().Be("PROD001");
        product.Name.Should().Be("Test Product");
        product.Description.Should().Be("Description");
        product.Price.Should().Be(99.99m);
        product.Stock.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCodeIsEmpty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new CreateProductCommandHandler(context, context);
        var command = new CreateProductCommand("", "Test Product", "Description", 99.99m, 10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Code cannot be empty");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCodeExceedsMaxLength()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new CreateProductCommandHandler(context, context);
        var command = new CreateProductCommand("TOOLONGCODE123456789X", "Test Product", "Description", 99.99m, 10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Code cannot exceed 20 characters");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCodeIsNotUnique()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new CreateProductCommandHandler(context, context);

        // Create the first product
        var firstCommand = new CreateProductCommand("PROD001", "First Product", "Description", 10m, 5);
        await handler.Handle(firstCommand, CancellationToken.None);

        // Attempt to create a second product with the same code
        var duplicateCommand = new CreateProductCommand("PROD001", "Second Product", "Description", 20m, 3);

        // Act
        var result = await handler.Handle(duplicateCommand, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("PROD001");
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
        var command = new CreateProductCommand("PROD001", "", "Description", 99.99m, 10);

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
        var command = new CreateProductCommand("PROD001", "Test Product", "Description", -10m, 10);

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
        var command = new CreateProductCommand("PROD001", "Test Product", "Description", 99.99m, -5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Stock cannot be negative");
    }
}

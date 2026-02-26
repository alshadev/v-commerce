using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Products.Commands.DeleteProduct;
using VCommerce.Domain.Products;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Infrastructure.Persistence.Configurations;

namespace VCommerce.Tests.Unit.Products.Features;

public class DeleteProductCommandHandlerTests
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
    public async Task Handle_ShouldSoftDeleteProduct_WithValidCommand()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        
        // Create a product first
        var product = Product.Create("PROD001", "Test Product", "Description", 99.99m, 10);
        context.Set<Product>().Add(product);
        await context.SaveChangesAsync();
        
        var handler = new DeleteProductCommandHandler(context, context);
        var command = new DeleteProductCommand(product.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedProduct = await context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();
        deletedProduct.DeletedAt.Should().NotBeNull();
        deletedProduct.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        var handler = new DeleteProductCommandHandler(context, context);
        var command = new DeleteProductCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductIsAlreadyDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        
        // Create and delete a product first
        var product = Product.Create("PROD001", "Test Product", "Description", 99.99m, 10);
        product.Delete();
        context.Set<Product>().Add(product);
        await context.SaveChangesAsync();
        
        var handler = new DeleteProductCommandHandler(context, context);
        var command = new DeleteProductCommand(product.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}

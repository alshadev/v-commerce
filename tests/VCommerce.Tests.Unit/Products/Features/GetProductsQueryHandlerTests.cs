using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Products.Queries.GetProducts;
using VCommerce.Domain.Products;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Infrastructure.Persistence.Configurations;

namespace VCommerce.Tests.Unit.Products.Features;

public class GetProductsQueryHandlerTests
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
    public async Task Handle_ShouldReturnOnlyNonDeletedProducts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        
        // Create products - some deleted, some not
        var product1 = Product.Create("PROD001", "Product 1", "Description 1", 10m, 5);
        var product2 = Product.Create("PROD002", "Product 2", "Description 2", 20m, 10);
        var product3 = Product.Create("PROD003", "Product 3", "Description 3", 30m, 15);
        
        product2.Delete(); // Soft delete product2
        
        context.Set<Product>().AddRange(product1, product2, product3);
        await context.SaveChangesAsync();
        
        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalItems.Should().Be(2);
        result.Value.Items.Should().Contain(p => p.Name == "Product 1");
        result.Value.Items.Should().Contain(p => p.Name == "Product 3");
        result.Value.Items.Should().NotContain(p => p.Name == "Product 2");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenAllProductsAreDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        await using var context = new TestApplicationDbContext(options);
        
        // Create products and delete them all
        var product1 = Product.Create("PROD001", "Product 1", "Description 1", 10m, 5);
        var product2 = Product.Create("PROD002", "Product 2", "Description 2", 20m, 10);
        
        product1.Delete();
        product2.Delete();
        
        context.Set<Product>().AddRange(product1, product2);
        await context.SaveChangesAsync();
        
        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalItems.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectPage_WhenPaginationIsApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestApplicationDbContext(options);

        for (int i = 1; i <= 15; i++)
        {
            context.Set<Product>().Add(
                Product.Create($"PROD{i:D3}", $"Product {i}", $"Description {i}", i * 10m, i));
        }
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery(Page: 2, PageSize: 5);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalItems.Should().Be(15);
        result.Value.TotalPages.Should().Be(3);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyCodeAndName_InListItems()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestApplicationDbContext(options);

        var product = Product.Create("PROD001", "Product 1", "Description 1", 10m, 5);
        context.Set<Product>().Add(product);
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);
        var query = new GetProductsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var item = result.Value!.Items.Single();
        item.Code.Should().Be("PROD001");
        item.Name.Should().Be("Product 1");
    }
}

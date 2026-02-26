using FluentAssertions;
using VCommerce.Domain.Products;

namespace VCommerce.Tests.Unit.Products.Domain;

public class ProductTests
{
    [Fact]
    public void Create_ShouldCreateProduct_WithValidData()
    {
        // Arrange
        var code = "PROD001";
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var stock = 10;

        // Act
        var product = Product.Create(code, name, description, price, stock);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Code.Should().Be(code);
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.Stock.Should().Be(stock);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCodeIsEmpty()
    {
        // Arrange & Act
        var act = () => Product.Create("", "Test Product", "Test Description", 99.99m, 10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Code cannot be empty*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenCodeExceedsMaxLength()
    {
        // Arrange & Act
        var act = () => Product.Create("TOOLONGCODE123456789X", "Test Product", "Test Description", 99.99m, 10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Code cannot exceed 20 characters*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var name = "";
        var description = "Test Description";
        var price = 99.99m;
        var stock = 10;

        // Act
        var act = () => Product.Create("PROD001", name, description, price, stock);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot be empty*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenNameExceedsMaxLength()
    {
        // Arrange & Act
        var act = () => Product.Create("PROD001", new string('A', 51), "Test Description", 99.99m, 10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name cannot exceed 50 characters*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenPriceIsNegative()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = -10m;
        var stock = 10;

        // Act
        var act = () => Product.Create("PROD001", name, description, price, stock);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Price cannot be negative*");
    }

    [Fact]
    public void Create_ShouldThrowArgumentException_WhenStockIsNegative()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 99.99m;
        var stock = -5;

        // Act
        var act = () => Product.Create("PROD001", name, description, price, stock);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Stock cannot be negative*");
    }

    [Fact]
    public void Update_ShouldUpdateProduct_WithValidData()
    {
        // Arrange
        var product = Product.Create("PROD001", "Original Name", "Original Description", 50m, 5);
        var newName = "Updated Name";
        var newDescription = "Updated Description";
        var newPrice = 75m;
        var newStock = 15;

        // Act
        product.Update(newName, newDescription, newPrice, newStock);

        // Assert
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
        product.Price.Should().Be(newPrice);
        product.Stock.Should().Be(newStock);
        product.UpdatedAt.Should().NotBeNull();
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AdjustStock_ShouldIncreaseStock_WhenQuantityIsPositive()
    {
        // Arrange
        var product = Product.Create("PROD001", "Test Product", "Description", 100m, 10);
        var quantity = 5;

        // Act
        product.AdjustStock(quantity);

        // Assert
        product.Stock.Should().Be(15);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AdjustStock_ShouldDecreaseStock_WhenQuantityIsNegative()
    {
        // Arrange
        var product = Product.Create("PROD001", "Test Product", "Description", 100m, 10);
        var quantity = -3;

        // Act
        product.AdjustStock(quantity);

        // Assert
        product.Stock.Should().Be(7);
    }

    [Fact]
    public void AdjustStock_ShouldThrowInvalidOperationException_WhenResultingStockIsNegative()
    {
        // Arrange
        var product = Product.Create("PROD001", "Test Product", "Description", 100m, 5);
        var quantity = -10;

        // Act
        var act = () => product.AdjustStock(quantity);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient stock");
    }

    [Fact]
    public void Delete_ShouldMarkProductAsDeleted_WhenProductIsNotDeleted()
    {
        // Arrange
        var product = Product.Create("PROD001", "Test Product", "Description", 100m, 10);

        // Act
        product.Delete();

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedAt.Should().NotBeNull();
        product.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_ShouldThrowInvalidOperationException_WhenProductIsAlreadyDeleted()
    {
        // Arrange
        var product = Product.Create("PROD001", "Test Product", "Description", 100m, 10);
        product.Delete();

        // Act
        var act = () => product.Delete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Product is already deleted");
    }
}

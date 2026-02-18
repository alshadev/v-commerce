namespace VCommerce.Modules.Products.Domain.Entities;

/// <summary>
/// Product entity following DDD principles
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Parameterless constructor for EF Core
    private Product() { }

    // Factory method for creating new products
    public static Product Create(string name, string description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business logic method for updating product
    public void Update(string name, string description, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));
        
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        UpdatedAt = DateTime.UtcNow;
    }

    // Business logic method for adjusting stock
    public void AdjustStock(int quantity)
    {
        if (Stock + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");

        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}

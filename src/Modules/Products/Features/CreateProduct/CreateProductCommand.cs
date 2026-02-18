using VCommerce.Infrastructure.Persistence;
using VCommerce.Modules.Products.Domain.Entities;
using VCommerce.Shared.Abstractions.CQRS;
using VCommerce.Shared.Common.Results;

namespace VCommerce.Modules.Products.Features.CreateProduct;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock
) : ICommand<Result<Guid>>;

/// <summary>
/// Handler for CreateProductCommand
/// </summary>
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    private readonly ApplicationDbContext _context;

    public CreateProductCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = Product.Create(
                request.Name,
                request.Description,
                request.Price,
                request.Stock
            );

            _context.Set<Product>().Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(product.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>($"An error occurred while creating the product: {ex.Message}");
        }
    }
}

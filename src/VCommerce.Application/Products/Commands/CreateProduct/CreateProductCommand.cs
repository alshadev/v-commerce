using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Common.Abstractions.CQRS;
using VCommerce.Application.Common.Interfaces;
using VCommerce.Domain.Common;
using VCommerce.Domain.Products;

namespace VCommerce.Application.Products.Commands.CreateProduct;

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
/// Handler for CreateProductCommand (Vertical Slice)
/// </summary>
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly DbContext _dbContext;

    public CreateProductCommandHandler(IApplicationDbContext context, DbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
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

            _dbContext.Set<Product>().Add(product);
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

using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Common.Abstractions.CQRS;
using VCommerce.Application.Common.Interfaces;
using VCommerce.Domain.Common;
using VCommerce.Domain.Products;

namespace VCommerce.Application.Products.Commands.UpdateProduct;

/// <summary>
/// Command to update a product
/// </summary>
public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock
) : ICommand<Result>;

/// <summary>
/// Handler for UpdateProductCommand (Vertical Slice)
/// </summary>
public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly DbContext _dbContext;

    public UpdateProductCommandHandler(IApplicationDbContext context, DbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result.Failure($"Product with ID {request.Id} not found");

            product.Update(
                request.Name,
                request.Description,
                request.Price,
                request.Stock
            );

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while updating the product: {ex.Message}");
        }
    }
}

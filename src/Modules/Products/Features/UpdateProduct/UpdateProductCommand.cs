using Microsoft.EntityFrameworkCore;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Modules.Products.Domain.Entities;
using VCommerce.Shared.Abstractions.CQRS;
using VCommerce.Shared.Common.Results;

namespace VCommerce.Modules.Products.Features.UpdateProduct;

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
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, Result>
{
    private readonly ApplicationDbContext _context;

    public UpdateProductCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Set<Product>()
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

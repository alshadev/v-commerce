using Microsoft.EntityFrameworkCore;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Modules.Products.Domain.Entities;
using VCommerce.Shared.Abstractions.CQRS;
using VCommerce.Shared.Common.Results;

namespace VCommerce.Modules.Products.Features.DeleteProduct;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(Guid Id) : ICommand<Result>;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Result>
{
    private readonly ApplicationDbContext _context;

    public DeleteProductCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result.Failure($"Product with ID {request.Id} not found");

            _context.Set<Product>().Remove(product);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while deleting the product: {ex.Message}");
        }
    }
}

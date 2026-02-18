using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Common.Abstractions.CQRS;
using VCommerce.Application.Common.Interfaces;
using VCommerce.Domain.Common;
using VCommerce.Domain.Products;

namespace VCommerce.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(Guid Id) : ICommand<Result>;

/// <summary>
/// Handler for DeleteProductCommand (Vertical Slice)
/// </summary>
public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly DbContext _dbContext;

    public DeleteProductCommandHandler(IApplicationDbContext context, DbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _dbContext.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (product == null)
                return Result.Failure($"Product with ID {request.Id} not found");

            _dbContext.Set<Product>().Remove(product);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred while deleting the product: {ex.Message}");
        }
    }
}

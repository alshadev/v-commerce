using Microsoft.EntityFrameworkCore;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Modules.Products.Domain.Entities;
using VCommerce.Shared.Abstractions.CQRS;
using VCommerce.Shared.Common.Results;

namespace VCommerce.Modules.Products.Features.GetProduct;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductQuery(Guid Id) : IQuery<Result<ProductDto>>;

/// <summary>
/// DTO for product response
/// </summary>
public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Handler for GetProductQuery
/// </summary>
public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly ApplicationDbContext _context;

    public GetProductQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
            return Result.Failure<ProductDto>($"Product with ID {request.Id} not found");

        var dto = new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CreatedAt,
            product.UpdatedAt
        );

        return Result.Success(dto);
    }
}

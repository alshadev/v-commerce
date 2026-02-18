using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Common.Abstractions.CQRS;
using VCommerce.Domain.Common;
using VCommerce.Domain.Products;

namespace VCommerce.Application.Products.Queries.GetProduct;

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
/// Handler for GetProductQuery (Vertical Slice)
/// </summary>
public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly DbContext _context;

    public GetProductQueryHandler(DbContext context)
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

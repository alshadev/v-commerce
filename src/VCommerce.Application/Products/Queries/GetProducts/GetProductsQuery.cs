using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Common.Abstractions.CQRS;
using VCommerce.Application.Products.Queries.GetProduct;
using VCommerce.Domain.Common;
using VCommerce.Domain.Products;

namespace VCommerce.Application.Products.Queries.GetProducts;

/// <summary>
/// Query to get all products
/// </summary>
public record GetProductsQuery : IQuery<Result<IEnumerable<ProductDto>>>;

/// <summary>
/// Handler for GetProductsQuery (Vertical Slice)
/// </summary>
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, Result<IEnumerable<ProductDto>>>
{
    private readonly DbContext _context;

    public GetProductsQueryHandler(DbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Set<Product>()
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new ProductDto(
                p.Id,
                p.Code,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProductDto>>(products);
    }
}

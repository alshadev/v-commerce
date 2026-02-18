using Microsoft.EntityFrameworkCore;
using VCommerce.Infrastructure.Persistence;
using VCommerce.Modules.Products.Domain.Entities;
using VCommerce.Modules.Products.Features.GetProduct;
using VCommerce.Shared.Abstractions.CQRS;
using VCommerce.Shared.Common.Results;

namespace VCommerce.Modules.Products.Features.GetProducts;

/// <summary>
/// Query to get all products
/// </summary>
public record GetProductsQuery : IQuery<Result<IEnumerable<ProductDto>>>;

/// <summary>
/// Handler for GetProductsQuery
/// </summary>
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, Result<IEnumerable<ProductDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetProductsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Set<Product>()
            .AsNoTracking()
            .Select(p => new ProductDto(
                p.Id,
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

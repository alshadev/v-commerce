using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Common;
using VCommerce.Application.Common.Abstractions.CQRS;
using VCommerce.Domain.Common;
using VCommerce.Domain.Products;

namespace VCommerce.Application.Products.Queries.GetProducts;

/// <summary>
/// DTO for product list item (code and name only)
/// </summary>
public record ProductListItemDto(string Code, string Name);

/// <summary>
/// Query to get a paginated list of products
/// </summary>
public record GetProductsQuery(int Page = 1, int PageSize = 10) : IQuery<Result<PaginatedResult<ProductListItemDto>>>;

/// <summary>
/// Handler for GetProductsQuery (Vertical Slice)
/// </summary>
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, Result<PaginatedResult<ProductListItemDto>>>
{
    private readonly DbContext _context;

    public GetProductsQueryHandler(DbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<ProductListItemDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Max(1, request.PageSize);

        var query = _context.Set<Product>()
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderBy(p => p.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemDto(p.Code, p.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(new PaginatedResult<ProductListItemDto>(
            items,
            totalItems,
            totalPages,
            page,
            pageSize
        ));
    }
}

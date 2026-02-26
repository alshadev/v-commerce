namespace VCommerce.Application.Common;

/// <summary>
/// Represents a paginated result set
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int TotalItems,
    int TotalPages,
    int Page,
    int PageSize
);

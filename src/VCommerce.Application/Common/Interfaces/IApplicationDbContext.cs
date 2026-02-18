namespace VCommerce.Application.Common.Interfaces;

/// <summary>
/// Application database context interface (Dependency Inversion)
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

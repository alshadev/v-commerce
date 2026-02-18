using Microsoft.EntityFrameworkCore;
using VCommerce.Application.Common.Interfaces;

namespace VCommerce.Infrastructure.Persistence;

/// <summary>
/// Main application DbContext implementing Clean Architecture principles
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations from Infrastructure assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

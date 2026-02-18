using Microsoft.EntityFrameworkCore;

namespace VCommerce.Infrastructure.Persistence;

/// <summary>
/// Main application DbContext
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations from all assemblies
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    /// <summary>
    /// Configure model from external assemblies
    /// </summary>
    public void ApplyModuleConfigurations(params System.Reflection.Assembly[] assemblies)
    {
        var modelBuilder = new ModelBuilder();
        foreach (var assembly in assemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }
    }
}

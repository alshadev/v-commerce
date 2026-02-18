using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VCommerce.Infrastructure.Persistence;

namespace VCommerce.Modules.Products;

/// <summary>
/// Extension methods for registering Products module services
/// </summary>
public static class ProductsModuleExtensions
{
    public static IServiceCollection AddProductsModule(this IServiceCollection services)
    {
        // Register module-specific services here if needed
        // For now, MediatR handlers are auto-registered by assembly scanning
        
        return services;
    }

    public static void ApplyProductsMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Apply migrations
        context.Database.Migrate();
    }
}

# VCommerce Architecture and Coding Guidelines

## Project Overview
VCommerce is an eCommerce platform built with .NET 10 following **Clean Architecture** principles combined with **Vertical Slice Architecture**, **CQRS**, and **Domain-Driven Design** (pragmatic DDD).

## Architecture Patterns

### 1. Clean Architecture
The application follows Clean Architecture (also known as Onion Architecture or Hexagonal Architecture) with clear separation of concerns and dependency inversion:

**Layer Structure (Dependencies point inward):**
- **Domain** (Core/Innermost) - No dependencies
- **Application** - Depends on Domain
- **Infrastructure** - Depends on Application & Domain
- **API** (Presentation/Outermost) - Depends on all layers

**Key Principles:**
- Dependencies flow inward toward the domain
- Domain layer has no external dependencies
- Business rules are encapsulated in the domain
- Application layer orchestrates domain objects
- Infrastructure implements interfaces defined in Application
- API layer is thin and delegates to Application layer

**Project Structure:**
```
src/
├── VCommerce.Domain/              # Core domain layer (no dependencies)
│   ├── Products/
│   │   └── Product.cs            # Domain entities
│   └── Common/
│       └── Result.cs             # Domain primitives
├── VCommerce.Application/         # Application layer (depends on Domain)
│   ├── Common/
│   │   ├── Abstractions/CQRS/   # CQRS interfaces
│   │   └── Interfaces/          # Infrastructure interfaces (DI)
│   └── Products/
│       ├── Commands/            # Use cases (commands)
│       └── Queries/             # Use cases (queries)
├── VCommerce.Infrastructure/      # Infrastructure layer (depends on Application)
│   └── Persistence/
│       ├── ApplicationDbContext.cs
│       └── Configurations/      # EF Core configurations
└── VCommerce.Api/                # Presentation layer (depends on all)
    ├── Endpoints/               # Minimal API endpoints
    └── Program.cs
```

### 2. Vertical Slice Architecture
- Features are organized by use case rather than technical layers
- Each feature is a vertical slice containing everything needed for that use case
- Features are located in `Application/Products/{Commands|Queries}/{FeatureName}`
- Each feature folder contains:
  - Command/Query definition
  - Handler implementation
  - DTOs specific to that feature
  - Validators (if needed)

**Example:**
```
Application/Products/
├── Commands/
│   ├── CreateProduct/
│   │   └── CreateProductCommand.cs (Command + Handler)
│   └── UpdateProduct/
│       └── UpdateProductCommand.cs (Command + Handler)
└── Queries/
    ├── GetProduct/
    │   └── GetProductQuery.cs (Query + Handler + DTO)
    └── GetProducts/
        └── GetProductsQuery.cs (Query + Handler)
```

### 3. CQRS (Command Query Responsibility Segregation)
- Uses **MediatR 12.4.1** for implementing CQRS pattern
- Commands modify state and may/may not return results
- Queries retrieve data without modifying state
- All commands implement `ICommand` or `ICommand<TResponse>`
- All queries implement `IQuery<TResponse>`
- Handlers implement `ICommandHandler<TCommand, TResponse>` or `IQueryHandler<TQuery, TResponse>`

**Command Example:**
```csharp
public record CreateProductCommand(string Name, string Description, decimal Price, int Stock) 
    : ICommand<Result<Guid>>;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly DbContext _dbContext;
    
    public CreateProductCommandHandler(IApplicationDbContext context, DbContext dbContext)
    {
        _context = context;
        _dbContext = dbContext;
    }
    
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.Stock);
        _dbContext.Set<Product>().Add(product);
        await _context.SaveChangesAsync(ct);
        return Result.Success(product.Id);
    }
}
```

**Query Example:**
```csharp
public record GetProductQuery(Guid Id) : IQuery<Result<ProductDto>>;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Result<ProductDto>>
{
    private readonly DbContext _context;
    
    public GetProductQueryHandler(DbContext context)
    {
        _context = context;
    }
    
    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken ct)
    {
        var product = await _context.Set<Product>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);
            
        if (product == null)
            return Result.Failure<ProductDto>($"Product with ID {request.Id} not found");
            
        return Result.Success(new ProductDto(product.Id, product.Name, ...));
    }
}
```

### 4. Domain-Driven Design (Pragmatic DDD)
- **Pragmatic DDD**: We apply DDD principles pragmatically, especially for simple CRUD operations
- **Entities**: Rich domain models with business logic encapsulated
- **Value Objects**: Used when appropriate for immutable domain concepts
- **Aggregates**: Entity clusters that maintain consistency boundaries
- **Domain Events**: (To be implemented as needed)

**Entity Guidelines:**
```csharp
// Domain/Products/Product.cs
namespace VCommerce.Domain.Products;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    
    // Private constructor for EF Core
    private Product() { }
    
    // Factory method for creation
    public static Product Create(string name, ...)
    {
        // Validation
        // Business rules
        return new Product { ... };
    }
    
    // Business logic methods
    public void Update(string name, ...)
    {
        // Validation
        // Business rules
    }
}
```

**Key DDD Principles:**
- Use factory methods for entity creation
- Encapsulate business logic within entities
- Use private setters to protect invariants
- Validate domain rules at the entity level
- Keep entities anemic only for simple CRUD when justified

## Dependency Inversion Principle

Clean Architecture enforces the Dependency Inversion Principle:

**Application Layer defines interfaces:**
```csharp
// Application/Common/Interfaces/IApplicationDbContext.cs
namespace VCommerce.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Infrastructure Layer implements interfaces:**
```csharp
// Infrastructure/Persistence/ApplicationDbContext.cs
namespace VCommerce.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    // Implementation
}
```

**Registration in API (Presentation) layer:**
```csharp
// VCommerce.Api/Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options => ...);
builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());
```

## Project Structure

```
/
├── src/
│   ├── VCommerce.Api/                  # API entry point (Minimal APIs)
│   │   ├── Endpoints/                  # Minimal API endpoints
│   │   └── Program.cs
│   ├── VCommerce.Domain/               # Domain layer (NO dependencies)
│   │   ├── Products/                   # Product aggregate
│   │   │   └── Product.cs
│   │   └── Common/                     # Domain primitives
│   │       └── Result.cs
│   ├── VCommerce.Application/          # Application layer (depends on Domain)
│   │   ├── Common/
│   │   │   ├── Abstractions/CQRS/     # CQRS abstractions
│   │   │   └── Interfaces/            # Infrastructure interfaces
│   │   └── Products/                   # Product use cases
│   │       ├── Commands/
│   │       │   ├── CreateProduct/
│   │       │   ├── UpdateProduct/
│   │       │   └── DeleteProduct/
│   │       └── Queries/
│   │           ├── GetProduct/
│   │           └── GetProducts/
│   └── VCommerce.Infrastructure/       # Infrastructure layer
│       └── Persistence/
│           ├── ApplicationDbContext.cs
│           └── Configurations/         # EF Core configurations
├── tests/
│   └── VCommerce.Tests.Unit/           # Unit tests
│       └── Products/
│           ├── Domain/                 # Domain logic tests
│           └── Features/               # Handler tests
└── .github/
    └── copilot-instructions.md
```

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    // Implementation
}
```

**Query Example:**
```csharp
public record GetProductQuery(Guid Id) : IQuery<Result<ProductDto>>;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, Result<ProductDto>>
{
    // Implementation
}
```

### 4. Domain-Driven Design (DDD)
- **Pragmatic DDD**: We apply DDD principles pragmatically, especially for simple CRUD operations
- **Entities**: Rich domain models with business logic encapsulated
- **Value Objects**: Used when appropriate for immutable domain concepts
- **Aggregates**: Entity clusters that maintain consistency boundaries
- **Domain Events**: (To be implemented as needed)

**Entity Guidelines:**
```csharp
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    
    // Private constructor for EF Core
    private Product() { }
    
    // Factory method for creation
    public static Product Create(string name, ...)
    {
        // Validation
        // Business rules
        return new Product { ... };
    }
    
    // Business logic methods
    public void Update(string name, ...)
    {
        // Validation
        // Business rules
    }
}
```

**Key DDD Principles:**
- Use factory methods for entity creation
- Encapsulate business logic within entities
- Use private setters to protect invariants
- Validate domain rules at the entity level
- Keep entities anemic only for simple CRUD when justified

## Project Structure

```
/
├── src/
│   ├── VCommerce.Api/                  # API entry point
│   │   ├── Endpoints/                  # Minimal API endpoints
│   │   └── Program.cs
│   ├── VCommerce.Shared/               # Shared kernel
│   │   ├── Abstractions/
│   │   │   └── CQRS/                   # CQRS abstractions
│   │   └── Common/
│   │       └── Results/                # Result pattern
│   ├── VCommerce.Infrastructure/       # Cross-cutting infrastructure
│   │   └── Persistence/
│   │       └── ApplicationDbContext.cs
│   └── Modules/
│       └── Products/                   # Products module
│           ├── Domain/
│           │   └── Entities/           # Domain entities
│           ├── Features/               # Vertical slices
│           │   ├── CreateProduct/
│           │   ├── GetProduct/
│           │   └── ...
│           ├── Persistence/
│           │   └── Configurations/     # EF Core configurations
│           └── ProductsModuleExtensions.cs
├── tests/
│   └── VCommerce.Tests.Unit/           # Unit tests
│       └── Products/
│           ├── Domain/                 # Domain logic tests
│           └── Features/               # Handler tests
└── .github/
    └── copilot-instructions.md
```

## Technology Stack

- **.NET 10**: Latest LTS version
- **ASP.NET Core**: Minimal APIs
- **Entity Framework Core 10**: ORM for data access
- **MediatR 12.4.1**: CQRS implementation
- **xUnit**: Unit testing framework
- **FluentAssertions**: Fluent assertion library
- **Moq**: Mocking framework

## Coding Guidelines

### 1. General Guidelines
- Follow C# coding conventions and best practices
- Use meaningful names for classes, methods, and variables
- Keep methods small and focused (Single Responsibility Principle)
- Use `record` for DTOs and immutable data
- Use `sealed` classes when inheritance is not intended
- Prefer composition over inheritance

### 2. CQRS Implementation
- **Commands** are operations that change state
- **Queries** are operations that retrieve data
- Never return domain entities directly from handlers; use DTOs
- Use the `Result<T>` pattern for error handling instead of exceptions for business logic failures
- Commands and Queries should be immutable (use `record`)

### 3. Domain Model
- Keep domain models rich with business logic
- Use factory methods (`Create`, `CreateFrom`) for entity creation
- Use private setters for properties
- Use private constructors for entity instantiation
- Validate business rules in the domain layer
- Throw exceptions for invariant violations
- Use domain events for cross-aggregate communication (when implemented)

### 4. Persistence
- Use **Code-First** approach with Entity Framework Core
- Create explicit configurations using `IEntityTypeConfiguration<T>`
- Store configurations in `{Module}/Persistence/Configurations/`
- Use database schemas to separate modules (e.g., `products`, `orders`)
- **Manual Migration**: Apply migrations manually using `dotnet ef migrations add` and `dotnet ef database update`

**Configuration Example:**
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        // ...
    }
}
```

### 5. API Endpoints
- Use **Minimal APIs** pattern
- Organize endpoints by module in static extension classes
- Keep endpoint logic thin; delegate to MediatR handlers
- Use appropriate HTTP verbs (GET, POST, PUT, DELETE)
- Return appropriate status codes (200, 201, 204, 400, 404)
- Use route groups to organize related endpoints

**Endpoint Example:**
```csharp
public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products").WithTags("Products");
        
        group.MapPost("/", CreateProduct);
        group.MapGet("/{id}", GetProductById);
        
        return endpoints;
    }
    
    private static async Task<IResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        [FromServices] IMediator mediator)
    {
        var command = new CreateProductCommand(request.Name, ...);
        var result = await mediator.Send(command);
        
        return result.IsSuccess
            ? Results.Created($"/api/products/{result.Value}", result.Value)
            : Results.BadRequest(new { error = result.Error });
    }
}
```

### 6. Testing
- Write unit tests for domain logic
- Write unit tests for command/query handlers
- Use in-memory database for handler tests
- Follow Arrange-Act-Assert (AAA) pattern
- Use descriptive test names: `MethodName_StateUnderTest_ExpectedBehavior`
- Use FluentAssertions for readable assertions

**Test Example:**
```csharp
[Fact]
public async Task Handle_ShouldCreateProduct_WithValidCommand()
{
    // Arrange
    var context = CreateInMemoryContext();
    var handler = new CreateProductCommandHandler(context);
    var command = new CreateProductCommand("Test Product", ...);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
}
```

### 7. Dependency Injection
- Register services in `Program.cs` or module extension methods
- Use constructor injection
- Register handlers automatically using MediatR assembly scanning
- Use appropriate service lifetimes (Transient, Scoped, Singleton)

### 8. Error Handling
- Use the `Result<T>` pattern for business logic errors
- Return `Result.Success()` or `Result.Failure(error)` from handlers
- Throw exceptions only for exceptional cases (infrastructure failures, invalid state)
- Catch exceptions at the handler level and convert to `Result.Failure`

## Adding a New Module

1. Create module folder structure:
```
src/Modules/{ModuleName}/
├── Domain/
│   └── Entities/
├── Features/
├── Persistence/
│   └── Configurations/
└── {ModuleName}ModuleExtensions.cs
```

2. Create domain entities with DDD principles
3. Create EF Core configurations
4. Implement features using Vertical Slice + CQRS
5. Create module registration extension
6. Register module in `Program.cs`
7. Create API endpoints
8. Write unit tests

## Adding a New Feature

1. Create feature folder in `src/Modules/{ModuleName}/Features/{FeatureName}/`
2. Create command/query class implementing `ICommand<T>` or `IQuery<T>`
3. Create handler class implementing `ICommandHandler<T>` or `IQueryHandler<T>`
4. Create DTOs if needed
5. Inject `ApplicationDbContext` or required services via constructor
6. Implement business logic
7. Create API endpoint
8. Write unit tests

## Migration Process (Manual)

1. Add migration:
```bash
dotnet ef migrations add MigrationName --project src/VCommerce.Infrastructure --startup-project src/VCommerce.Api
```

2. Review generated migration
3. Apply migration:
```bash
dotnet ef database update --project src/VCommerce.Infrastructure --startup-project src/VCommerce.Api
```

4. For production, generate SQL scripts:
```bash
dotnet ef migrations script --project src/VCommerce.Infrastructure --startup-project src/VCommerce.Api --output migration.sql
```

## Best Practices Summary

1. **Separation of Concerns**: Keep domain logic separate from infrastructure
2. **Modularity**: Each module should be independent and cohesive
3. **Testability**: Write testable code using dependency injection
4. **Immutability**: Use records for DTOs and value objects
5. **Encapsulation**: Hide implementation details, expose only what's needed
6. **SOLID Principles**: Follow Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion
7. **DRY**: Don't Repeat Yourself - extract common logic
8. **YAGNI**: You Aren't Gonna Need It - don't over-engineer
9. **KISS**: Keep It Simple, Stupid - prefer simple solutions

## Code Review Checklist

When reviewing code, ensure:
- [ ] Follows architectural patterns (Modular Monolith, Vertical Slice, CQRS, DDD)
- [ ] Domain logic is encapsulated in entities
- [ ] Commands/Queries use MediatR and Result pattern
- [ ] EF Core configurations are explicit and complete
- [ ] API endpoints are thin and delegate to handlers
- [ ] Unit tests cover domain logic and handlers
- [ ] Code follows C# conventions and guidelines
- [ ] No business logic in API controllers/endpoints
- [ ] Proper error handling with Result pattern
- [ ] Dependencies are injected via constructor

## Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Vertical Slice Architecture by Jimmy Bogard](https://www.youtube.com/watch?v=SUiWfhAhgQw)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

---

**Note**: This is a living document. Update it as the architecture evolves and new patterns are introduced.

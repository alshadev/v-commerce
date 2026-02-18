using MediatR;
using Microsoft.AspNetCore.Mvc;
using VCommerce.Modules.Products.Features.CreateProduct;
using VCommerce.Modules.Products.Features.DeleteProduct;
using VCommerce.Modules.Products.Features.GetProduct;
using VCommerce.Modules.Products.Features.GetProducts;
using VCommerce.Modules.Products.Features.UpdateProduct;

namespace VCommerce.Api.Endpoints;

/// <summary>
/// Extension methods for registering Products API endpoints
/// </summary>
public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();

        group.MapGet("/", GetAllProducts)
            .WithName("GetAllProducts")
            .WithSummary("Get all products");

        group.MapGet("/{id:guid}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by ID");

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product");

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update a product");

        group.MapDelete("/{id:guid}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product");

        return endpoints;
    }

    private static async Task<IResult> GetAllProducts(
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery();
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error });
    }

    private static async Task<IResult> GetProductById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(new { error = result.Error });
    }

    private static async Task<IResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Stock);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/products/{result.Value}", new { id = result.Value })
            : Results.BadRequest(new { error = result.Error });
    }

    private static async Task<IResult> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Stock);

        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(new { error = result.Error });
    }

    private static async Task<IResult> DeleteProduct(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(new { error = result.Error });
    }
}

// Request DTOs
public record CreateProductRequest(string Name, string Description, decimal Price, int Stock);
public record UpdateProductRequest(string Name, string Description, decimal Price, int Stock);

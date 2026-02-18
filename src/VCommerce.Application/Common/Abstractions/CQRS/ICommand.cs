using MediatR;

namespace VCommerce.Application.Common.Abstractions.CQRS;

/// <summary>
/// Marker interface for commands that don't return a result
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
/// <typeparam name="TResponse">The type of the response</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

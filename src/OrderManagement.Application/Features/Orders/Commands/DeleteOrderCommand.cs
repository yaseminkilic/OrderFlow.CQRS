using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.CQRS.Application.Common.Models;
using OrderFlow.CQRS.Domain.Exceptions;
using OrderFlow.CQRS.Domain.Interfaces;

namespace OrderFlow.CQRS.Application.Features.Orders.Commands;

public record DeleteOrderCommand(Guid OrderId) : IRequest<Result>;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ILogger<DeleteOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Order), request.OrderId);

        await _orderRepository.DeleteAsync(request.OrderId, cancellationToken);

        _logger.LogInformation("Order {OrderNumber} deleted", order.OrderNumber);

        return Result.Success();
    }
}

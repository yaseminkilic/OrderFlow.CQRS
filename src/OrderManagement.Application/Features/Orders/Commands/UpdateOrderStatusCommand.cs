using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.CQRS.Application.Common.Models;
using OrderFlow.CQRS.Application.DTOs;
using OrderFlow.CQRS.Application.Interfaces;
using OrderFlow.CQRS.Domain.Enums;
using OrderFlow.CQRS.Domain.Events;
using OrderFlow.CQRS.Domain.Exceptions;
using OrderFlow.CQRS.Domain.Interfaces;

namespace OrderFlow.CQRS.Application.Features.Orders.Commands;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<Result<OrderDto>>;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        IMediator mediator,
        IMessagePublisher messagePublisher,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _mediator = mediator;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Order), request.OrderId);

        var oldStatus = order.Status;
        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order, cancellationToken);

        _logger.LogInformation("Order {OrderNumber} status changed from {OldStatus} to {NewStatus}",
            order.OrderNumber, oldStatus, request.NewStatus);

        await _mediator.Publish(new OrderStatusChangedEvent(
            order.Id, order.OrderNumber, oldStatus, request.NewStatus), cancellationToken);

        await _messagePublisher.PublishAsync(new
        {
            order.Id,
            order.OrderNumber,
            OldStatus = oldStatus.ToString(),
            NewStatus = request.NewStatus.ToString(),
            Event = "OrderStatusChanged"
        }, "order-events", cancellationToken);

        return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }
}

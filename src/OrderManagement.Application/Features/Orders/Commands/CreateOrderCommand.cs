using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.CQRS.Application.Common.Models;
using OrderFlow.CQRS.Application.DTOs;
using OrderFlow.CQRS.Application.Interfaces;
using OrderFlow.CQRS.Domain.Entities;
using OrderFlow.CQRS.Domain.Events;
using OrderFlow.CQRS.Domain.Interfaces;

namespace OrderFlow.CQRS.Application.Features.Orders.Commands;

public class CreateOrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public record CreateOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    string? Notes,
    List<CreateOrderItemDto> Items) : IRequest<Result<OrderDto>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        IMediator mediator,
        IMessagePublisher messagePublisher,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _mediator = mediator;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = _mapper.Map<Order>(request);
        order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);

        _logger.LogInformation("Order {OrderNumber} created successfully for {CustomerEmail}",
            createdOrder.OrderNumber, createdOrder.CustomerEmail);

        // Publish domain event
        await _mediator.Publish(new OrderCreatedEvent(
            createdOrder.Id, createdOrder.OrderNumber, createdOrder.CustomerEmail), cancellationToken);

        // Publish to RabbitMQ
        await _messagePublisher.PublishAsync(new
        {
            createdOrder.Id,
            createdOrder.OrderNumber,
            createdOrder.CustomerEmail,
            createdOrder.TotalAmount,
            Event = "OrderCreated"
        }, "order-events", cancellationToken);

        return Result<OrderDto>.Success(_mapper.Map<OrderDto>(createdOrder));
    }
}

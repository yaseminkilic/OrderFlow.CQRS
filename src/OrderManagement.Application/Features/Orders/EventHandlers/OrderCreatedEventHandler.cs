using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.CQRS.Domain.Events;

namespace OrderFlow.CQRS.Application.Features.Orders.EventHandlers;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Domain Event: Order {OrderNumber} created for customer {CustomerEmail}",
            notification.OrderNumber,
            notification.CustomerEmail);

        // Burada e-posta gönderme, bildirim vb. işlemler yapılabilir
        return Task.CompletedTask;
    }
}

using MediatR;
using OrderFlow.CQRS.Domain.Enums;

namespace OrderFlow.CQRS.Domain.Events;

public record OrderStatusChangedEvent(
    Guid OrderId,
    string OrderNumber,
    OrderStatus OldStatus,
    OrderStatus NewStatus) : INotification;

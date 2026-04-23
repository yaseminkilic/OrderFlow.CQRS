using MediatR;

namespace OrderFlow.CQRS.Domain.Events;

public record OrderCreatedEvent(Guid OrderId, string OrderNumber, string CustomerEmail) : INotification;

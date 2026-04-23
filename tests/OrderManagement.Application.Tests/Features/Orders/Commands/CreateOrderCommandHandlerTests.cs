using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.CQRS.Application.Features.Orders.Commands;
using OrderFlow.CQRS.Application.Interfaces;
using OrderFlow.CQRS.Application.Mappings;
using OrderFlow.CQRS.Application.Tests.TestData;
using OrderFlow.CQRS.Domain.Entities;
using OrderFlow.CQRS.Domain.Events;
using OrderFlow.CQRS.Domain.Interfaces;

namespace OrderFlow.CQRS.Application.Tests.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
    private readonly IMapper _mapper;
    private readonly CreateOrderCommandHandler _sut;

    public CreateOrderCommandHandlerTests()
    {
        _mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        _repository.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Order>());

        _sut = new CreateOrderCommandHandler(
            _repository, _mapper, _mediator, _publisher,
            NullLogger<CreateOrderCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessResult()
    {
        var command = CreateOrderCommandFaker.Valid(
            CreateOrderCommandFaker.Item(quantity: 2, unitPrice: 50m),
            CreateOrderCommandFaker.Item(quantity: 1, unitPrice: 25m));

        var result = await _sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalAmount.Should().Be(125m);
        result.Data.OrderNumber.Should().StartWith("ORD-");
    }

    [Fact]
    public async Task Handle_WithValidCommand_PersistsOrderExactlyOnce()
    {
        var command = CreateOrderCommandFaker.Valid();

        await _sut.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_PublishesOrderCreatedDomainEvent()
    {
        var command = CreateOrderCommandFaker.Valid();

        await _sut.Handle(command, CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<OrderCreatedEvent>(e => e.CustomerEmail == command.CustomerEmail),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_PublishesIntegrationEventToOrderEventsQueue()
    {
        var command = CreateOrderCommandFaker.Valid();

        await _sut.Handle(command, CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            Arg.Any<object>(), "order-events", Arg.Any<CancellationToken>());
    }
}

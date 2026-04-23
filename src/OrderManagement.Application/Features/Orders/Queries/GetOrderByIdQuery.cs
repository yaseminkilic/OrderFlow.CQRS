using AutoMapper;
using MediatR;
using OrderFlow.CQRS.Application.Common.Models;
using OrderFlow.CQRS.Application.DTOs;
using OrderFlow.CQRS.Domain.Exceptions;
using OrderFlow.CQRS.Domain.Interfaces;

namespace OrderFlow.CQRS.Application.Features.Orders.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<OrderDto>>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Order), request.OrderId);

        return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }
}

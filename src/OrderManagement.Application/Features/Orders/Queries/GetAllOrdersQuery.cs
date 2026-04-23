using AutoMapper;
using MediatR;
using OrderFlow.CQRS.Application.Common.Models;
using OrderFlow.CQRS.Application.DTOs;
using OrderFlow.CQRS.Domain.Interfaces;

namespace OrderFlow.CQRS.Application.Features.Orders.Queries;

public record GetAllOrdersQuery : IRequest<Result<IReadOnlyList<OrderDto>>>;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<IReadOnlyList<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        var orderDtos = _mapper.Map<IReadOnlyList<OrderDto>>(orders);
        return Result<IReadOnlyList<OrderDto>>.Success(orderDtos);
    }
}

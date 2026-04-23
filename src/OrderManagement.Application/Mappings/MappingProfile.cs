using AutoMapper;
using OrderFlow.CQRS.Application.DTOs;
using OrderFlow.CQRS.Application.Features.Orders.Commands;
using OrderFlow.CQRS.Domain.Entities;

namespace OrderFlow.CQRS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<CreateOrderCommand, Order>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));
        CreateMap<CreateOrderItemDto, OrderItem>();
    }
}

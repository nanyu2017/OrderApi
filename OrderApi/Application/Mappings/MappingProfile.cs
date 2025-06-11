using AutoMapper;
using OrderApi.Application.DTOs;
using OrderApi.Domain.Entities;

namespace OrdersManagementApi.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateOrderRequest, Order>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<OrderItemRequest, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore());

            CreateMap<Order, CreateOrderResponse>()
                .ForMember(dest => dest.Message, opt => opt.Ignore());
        }
    }
}

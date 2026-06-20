using AutoMapper;
using BookShop.API.DTOs.Order;
using BookShop.API.Models.Catalog;
using BookShop.API.Models.Order;

namespace BookShop.API.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Item, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrderId, opt => opt.Ignore())
            .ForMember(dest => dest.Order, opt => opt.Ignore());

        CreateMap<OrderItem, OrderItemDto>();

        CreateMap<Order, OrderDto>()
            .ForCtorParam("status", opt => opt.MapFrom(src => src.Status.ToString()));
    }
}

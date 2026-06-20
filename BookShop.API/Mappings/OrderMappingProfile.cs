using AutoMapper;
using BookShop.API.DTOs.Order;
using BookShop.API.Models.Catalog;
using BookShop.API.Models.Order;

namespace BookShop.API.Mappings;

/// <summary>
/// Defines AutoMapper mapping configuration for order-related types.
/// </summary>
/// <remarks>
/// Maps <see cref="Item"/> to <see cref="OrderItem"/> capture a snapshot of cart item deatils at the time of order creation.
/// Maps <see cref="Order"/> to <see cref="OrderDto"/> converting <see cref="OrderStatus"/> to its string representation.
/// </remarks>
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

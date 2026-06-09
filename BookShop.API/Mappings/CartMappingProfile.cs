using AutoMapper;
using BookShop.API.DTOs.Catalog;
using BookShop.API.Models.Catalog;

namespace BookShop.API.Mappings;

/// <summary>
/// AutoMapper profile that defines mappings between <see cref="Cart"/> and <see cref="CartDto"/>, and between <see cref="Item"/> 
/// and <see cref="CartItemDto"/>.    
/// </summary>
/// <remarks>
/// The <see cref="Cart"/> to <see cref="CartDto"/> mapping computes <c>TotalPrice</c> as the sum of each item's price multiplied 
/// by its quantity.
/// </remarks>
public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<CartDto, Cart>();

        CreateMap<Cart, CartDto>()
            .ForCtorParam("totalPrice", opt => opt.MapFrom(src => src.Items.Sum(i => i.Price * i.Quantity)));

        CreateMap<CartItemDto, Item>();

        CreateMap<Item, CartItemDto>();
    }
}
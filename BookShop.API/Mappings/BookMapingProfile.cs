using AutoMapper;
using BookShop.API.Models;

namespace BookShop.API.Mappings;

/// <summary>
/// Defines AutoMapper configuration for mapping between Book and BookDto objects.
/// </summary>
/// <remarks>This profile is intended to be used with AutoMapper to facilitate object-to-object mapping in
/// scenarios where Book entities need to be converted to BookDto data transfer objects. Register this profile with the
/// AutoMapper configuration to enable the mapping.</remarks>
public class BookMapingProfile : Profile
{
    public BookMapingProfile()
    {
        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id!.ToString()));

        CreateMap<Book, BookSearchRequestDto>();

    }
}

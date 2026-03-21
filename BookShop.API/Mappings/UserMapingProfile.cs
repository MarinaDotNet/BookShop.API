using AutoMapper;
using BookShop.API.Models.Auth;
using BookShop.API.DTOs.Auth;

namespace BookShop.API.Mappings;

/// <summary>
/// Defines AutoMapper configuration for mappings between user entities and authentication-related user DTOs.
/// </summary>
public class UserMapingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserMapingProfile"/> class and configures mappings for user models.
    /// </summary>
    public UserMapingProfile()
    {
        CreateMap<User, UserDto>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email))
            .ForCtorParam("UserName", opt => opt.MapFrom(src => src.UserName))
            .ForCtorParam("Roles", opt => opt.MapFrom(src => src.UserRoles.Select(r => r.Role.Name)))
            .ForCtorParam("IsEmailConfirmed", opt => opt.MapFrom(src => src.IsEmailConfirmed));

        CreateMap<UserDto, User>();
    }
}
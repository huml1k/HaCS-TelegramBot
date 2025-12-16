using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Contracts.AutoMapping
{
    public class UserMapping : Profile
    {
        public UserMapping()
		{
			CreateMap<User, UserDto>()
				.ForMember(dest => dest.FullName,
					opt => opt.MapFrom(src =>
						$"{src.LastName} {src.FirstName} {src.MiddleName}".Trim()));

			CreateMap<User, UserProfileDto>()
				.ForMember(dest => dest.FullName,
					opt => opt.MapFrom(src =>
						$"{src.LastName} {src.FirstName} {src.MiddleName}".Trim()))
				.ForMember(dest => dest.Apartments,
					opt => opt.MapFrom(src =>
						src.Apartments.Select(a => a.ApartmentNumber).ToList()));

			CreateMap<User, AuthorizationResultDto>()
				.ForMember(dest => dest.Success,
					opt => opt.MapFrom(src =>
						src.IsAuthorizedInBot));
		}
	}
}

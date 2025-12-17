using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Contracts.AutoMapping
{
	public class ApartmentMapping : Profile
	{
		public ApartmentMapping()
		{
            CreateMap<Apartment, ApartmentDto>()
            .ForMember(dest => dest.Number,
                opt => opt.MapFrom(src => src.ApartmentNumber))
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src => src.User != null
                    ? $"{src.User.LastName} {src.User.FirstName} {src.User.MiddleName}".Trim()
                    : string.Empty))
            .ForMember(dest => dest.BuildingAddress,
                opt => opt.MapFrom(src => src.Building != null
                    ? $"{src.Building.StreetType} {src.Building.StreetName}, д. {src.Building.BuildingNumber}".Trim()
                    : string.Empty))
            .ForMember(dest => dest.UserId,
                opt => opt.MapFrom(src => src.UserId));
        }

	}
}

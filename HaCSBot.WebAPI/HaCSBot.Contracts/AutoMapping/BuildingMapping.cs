using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Contracts.AutoMapping
{
	public class BuildingMapping : Profile
	{
		public BuildingMapping()
		{
			CreateMap<Building, BuildingDto>()
				.ForMember(dest => dest.FullAddress,
					opt => opt.MapFrom(src =>
					$"{src.StreetType} {src.StreetName}, {src.BuildingNumber}"));
		}
	}
}

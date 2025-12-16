using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.Contracts.AutoMapping
{
	public class TariffMapper : Profile
	{
		public TariffMapper()
		{
			CreateMap<Tariff, TariffDto>()
				.ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Building))
				.ReverseMap(); // Если нужно маппить обратно

			// Если используете BuildingDto
			CreateMap<Building, BuildingDto>().ReverseMap();
		}
	}
}

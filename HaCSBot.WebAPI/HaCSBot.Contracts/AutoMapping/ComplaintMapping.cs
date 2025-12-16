using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.Contracts.AutoMapping
{
	public class ComplaintMapping : Profile
	{
		public ComplaintMapping()
		{
			CreateMap<CreateComplaintDto, Complaint>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ComplaintStatus.New))
				.ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
				.ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
				.ForMember(dest => dest.Apartment, opt => opt.Ignore())
				.ForMember(dest => dest.ResolvedDate, opt => opt.Ignore());

			CreateMap<Complaint, ComplaintDto>()
				.ForMember(dest => dest.Attachments,
					opt => opt.MapFrom(src =>
						src.Attachments.Select(a => a.TelegramFileId).ToList()));

			CreateMap<Complaint, ComplaintDetailsDto>()
				.ForMember(dest => dest.ApartmentNumber,
					opt => opt.MapFrom(src => src.Apartment.ApartmentNumber))
				.ForMember(dest => dest.BuildingAddress,
					opt => opt.MapFrom(src =>
						$"{src.Apartment.Building.StreetType} {src.Apartment.Building.StreetName}, {src.Apartment.Building.BuildingNumber}"))
				.ForMember(dest => dest.Attachments,
					opt => opt.MapFrom(src => src.Attachments));

			CreateMap<AttachmentDto, ComplaintAttachment>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
				.ForMember(dest => dest.ComplaintId, opt => opt.Ignore())
				.ForMember(dest => dest.Complaint, opt => opt.Ignore());

			CreateMap<ComplaintAttachment, AttachmentDto>();
		}
	}
}

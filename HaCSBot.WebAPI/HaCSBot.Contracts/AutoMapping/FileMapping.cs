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
	public class FileMappingProfile : Profile
	{
		public FileMappingProfile()
		{
			CreateMap<AttachmentDto, ComplaintAttachment>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
				.ForMember(dest => dest.ComplaintId, opt => opt.Ignore())
				.ForMember(dest => dest.Complaint, opt => opt.Ignore());

			CreateMap<AttachmentDto, NotificationAttachment>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
				.ForMember(dest => dest.NotificationId, opt => opt.Ignore())
				.ForMember(dest => dest.Notification, opt => opt.Ignore());

			CreateMap<ComplaintAttachment, AttachmentDto>();

			CreateMap<NotificationAttachment, AttachmentDto>();

			CreateMap<AttachmentDto, SendFileDto>()
				.ForMember(dest => dest.TelegramId, opt => opt.Ignore()); 
		}
	}
}

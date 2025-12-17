using AutoMapper;
using HaCSBot.Contracts.DTOs;
using HaCSBot.DataBase.Models;

namespace HaCSBot.Contracts.AutoMapping
{
    public class NotificationMapping : Profile
    {
        public NotificationMapping()
        {
            CreateMap<CreateNotificationDto, Notification>()
               .ForMember(dest => dest.Attachments, opt => opt.Ignore()) // Игнорируем вложения
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
               .ForMember(dest => dest.ScheduledSendDate, opt => opt.Ignore())
               .ForMember(dest => dest.Deliveries, opt => opt.Ignore())
               .ForMember(dest => dest.Building, opt => opt.Ignore())
               .ForMember(dest => dest.BuildingMaintenance, opt => opt.Ignore())
               .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore());

            // Notification -> NotificationDto
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.SentDate,
                    opt => opt.MapFrom(src => src.ScheduledSendDate ?? src.CreatedDate));

            // Notification -> NotificationAdminDto
            CreateMap<Notification, NotificationAdminDto>()
                .ForMember(dest => dest.ReadCount,
                    opt => opt.MapFrom(src => src.Deliveries.Count(d => d.ReadDate != null)))
                .ForMember(dest => dest.ReadCount,
                    opt => opt.MapFrom(src => src.Deliveries.Count(d => d.SentDate != null)))
                .ForMember(dest => dest.ReadCount,
                    opt => opt.MapFrom(src => src.Deliveries.Count));
        }
    }
}

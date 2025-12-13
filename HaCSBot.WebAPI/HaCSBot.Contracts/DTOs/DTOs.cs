using HaCSBot.DataBase.Enums;
using System.ComponentModel.DataAnnotations;

namespace HaCSBot.Contracts.DTOs
{
    public class DTOs
    {
        public class AuthorizationResult
        {
            public bool Success { get; set; }
            public Guid UserId { get; set; }
        }

        public class ApartmentInfoDto
        {
            public Guid Id { get; set; }
            public string Number { get; set; } = string.Empty;
            public string BuildingAddress { get; set; } = string.Empty;
        }

        public class UserProfileDto
        {
            public string FullName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public Roles Role { get; set; }
            public List<string> Apartments { get; set; } = new List<string>();
        }

        // Для IBuildingService
        public class BuildingDto
        {
            public Guid Id { get; set; }
            public string FullAddress { get; set; } = string.Empty;
        }

        public class ApartmentDto
        {
            public Guid Id { get; set; }
            public string Number { get; set; } = string.Empty;
            public string? OwnerName { get; set; }
        }

        // Для INotificationService
        public class CreateNotificationDto
        {
            [Required]
            public NotificationType Type { get; set; }
            [Required]
            public string Message { get; set; } = string.Empty;
            public Guid? BuildingId { get; set; }
            public Guid? BuildingMaintenanceId { get; set; }
            public DateTime? ScheduledSendDate { get; set; }
            public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
        }

        public class NotificationDto
        {
            public Guid Id { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime? SentDate { get; set; }
        }

        public class NotificationAdminDto
        {
            public Guid Id { get; set; }
            public string Message { get; set; } = string.Empty;
            public int ReadCount { get; set; }
        }

        // Для IComplaintService
        public class CreateComplaintDto
        {
            [Required]
            public Guid ApartmentId { get; set; }
            [Required]
            public ComplaintCategory Category { get; set; }
            [Required]
            public string Description { get; set; } = string.Empty;
            public List<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
        }

        public class ComplaintDto
        {
            public Guid Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public ComplaintStatus Status { get; set; }
        }

        public class ComplaintDetailsDto
        {
            public Guid Id { get; set; }
            public string Description { get; set; } = string.Empty;
            public List<string> Attachments { get; set; } = new List<string>(); // TelegramFileId
        }

        // Для ITariffService
        public class TariffDto
        {
            public TariffType Type { get; set; }
            public decimal Price { get; set; } // Предполагаемое поле
        }

        // Для IMeterReadingService
        public class SubmitReadingDto
        {
            [Required]
            public Guid ApartmentId { get; set; }
            [Required]
            public MeterType Type { get; set; }
            [Required]
            public decimal Value { get; set; }
        }

        public class MeterReadingDto
        {
            public MeterType Type { get; set; }
            public decimal Value { get; set; }
            public DateTime Date { get; set; }
        }

        public class MeterReadingHistoryDto
        {
            public MeterType Type { get; set; }
            public decimal Value { get; set; }
            public DateTime Date { get; set; }
        }

        public class ConsumptionDto
        {
            public decimal Water { get; set; } // Пример для расхода воды
            public decimal Electricity { get; set; } // Пример для электричества
                                                     // Добавьте другие типы по MeterType
        }

        // Для IAdminService
        public class UserDto
        {
            public Guid Id { get; set; }
            public string FullName { get; set; } = string.Empty;
        }

        public class DashboardStatsDto
        {
            public int ResidentsCount { get; set; }
            public int ComplaintsCount { get; set; }
            public int NotificationsCount { get; set; }
        }

        // Для IFileService и общих вложений
        public class AttachmentDto
        {
            public AttachmentType Type { get; set; }
            public string TelegramFileId { get; set; } = string.Empty;
            public string? Caption { get; set; }
        }

        public class AttachmentInfo
        {
            public string TelegramFileId { get; set; } = string.Empty;
            public AttachmentType Type { get; set; }
            public string? Caption { get; set; }
        }
    }
}

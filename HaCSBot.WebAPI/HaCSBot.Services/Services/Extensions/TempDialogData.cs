using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using static HaCSBot.Contracts.DTOs.DTOs;

namespace HaCSBot.Services.Services.Extensions
{
    public class TempDialogData
    {
        // === Регистрация ===
        public string? RegFirstName { get; set; }
        public string? RegLastName { get; set; }
        public string? RegPhone { get; set; }

        // === Жалоба ===
        public Guid? CompApartmentId { get; set; }
        public ComplaintCategory? CompCategory { get; set; }
        public string? CompDescription { get; set; }
        public List<AttachmentInfo> CompAttachments { get; set; } = new();

        // === Передача показаний ===
        public List<Apartment>? MeterApartments { get; set; } = new();
        public Guid? MeterApartmentId { get; set; }
        public MeterType? MeterType { get; set; }

        // === Тарифы ===
        public string? TariffAddressInput { get; set; }
        public Guid? TariffBuildingId { get; set; }
    }
}

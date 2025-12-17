using HaCSBot.DataBase.Enums;

namespace HaCSBot.Contracts.DTOs
{
    public class BuildingForNotificationDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string StreetName { get; set; }
        public string BuildingNumber { get; set; }
    }
}

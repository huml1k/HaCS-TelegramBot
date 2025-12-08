using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class Notification
    {
        public Guid ID { get; set; }
        public DateTime? SentDate { get; set; } = DateTime.UtcNow;
        public NotificationType Type { get; set; }
        public string Message { get; set; }

        public Guid BuildingId { get; set; }
        public Guid BuildingMaintenanceId { get; set; }
        public Building  House { get; set; }
        public BuildingMaintenance BuildingMaintenance { get; set; }

        public static Notification Create(Notification notification) 
        {
            throw new Exception();
        }
    }
}

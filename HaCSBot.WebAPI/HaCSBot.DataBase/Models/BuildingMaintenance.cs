using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class BuildingMaintenance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public MaintenanceTypes Type { get; set; }
        public string Description { get; set; }
        public StatusMaintenance Status { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? PlannedEndDate { get; set; }

        public Guid BuildingId { get; set; }
        public Guid AdminId { get; set; }
        public Guid UserId { get; set; }
        public Building Building { get; set; }
        public User User { get; set; }
    }
}

using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class BuildingMaintenance
    {
        public Guid ID { get; set; } = Guid.NewGuid();

        public MaintenanceTypes MaintenanceTypes { get; set; }

        public string Discription { get; set; }

        public StatusMaintenance Status { get; set; }

        public static BuildingMaintenance Create(BuildingMaintenance buildingMaintenance) 
        {
            return new BuildingMaintenance 
            {
                ID = buildingMaintenance.ID,
                MaintenanceTypes = buildingMaintenance.MaintenanceTypes,
                Discription = buildingMaintenance.Discription,
                Status = buildingMaintenance.Status,
            };
        }
    }
}

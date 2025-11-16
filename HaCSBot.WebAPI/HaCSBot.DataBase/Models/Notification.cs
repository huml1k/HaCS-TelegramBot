namespace HaCSBot.DataBase.Models
{
    public class Notification
    {
        public Guid ID { get; set; }

        public BuildingMaintenance BuildingMaintenance { get; set; }

        public House  House { get; set; }

        public static Notification Create(Notification notification) 
        {
            throw new Exception();
        }
    }
}

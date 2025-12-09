using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class Building
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public StreetsType StreetType { get; set; }
        public string StreetName { get; set; }
        public string BuildingNumber { get; set; }
        public List<Apartment> Apartments { get; set; }
    }
}

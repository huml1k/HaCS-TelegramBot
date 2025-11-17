using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class Building
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public StreetsType StreetType { get; set; }
        public string StreetName { get; set; }
        public string BuildingNumber { get; set; }


        public static Building Create(Building house) 
        {
            return new Building
            {
                Id = house.Id,
                StreetType = house.StreetType,
                StreetName = house.StreetName,
                BuildingNumber = house.BuildingNumber,
            };
        }
    }
}

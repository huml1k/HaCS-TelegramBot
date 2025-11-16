using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class House
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public StreetsType StreetsType { get; set; }

        public string StreetNumber { get; set; }

        public string Apartment {  get; set; }

        public static House Create(House house) 
        {
            return new House
            {
                Id = house.Id,
                StreetsType = house.StreetsType,
                StreetNumber = house.StreetNumber,
                Apartment = house.Apartment,
            };
        }
    }
}

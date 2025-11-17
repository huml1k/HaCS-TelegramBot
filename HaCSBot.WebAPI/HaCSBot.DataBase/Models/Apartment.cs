namespace HaCSBot.DataBase.Models
{
    public class Apartment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string ApartmentNumber { get; set; }

        public Guid BuildingId { get; set; }
        public Guid UserId { get; set; }
        public Building Building { get; set; }
        public User User { get; set; }
    }
}

using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string MiddleName { get; set; }
        public required string Phone { get; set; }
        public Roles Role { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<Apartment> Apartments { get; set; }

        public User Create(User user)
        {
            return new User
            {
                Id = user.Id,
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Phone = user.Phone,
                Role = user.Role,
                CreatedDate = user.CreatedDate,
            };
        }
    }
}

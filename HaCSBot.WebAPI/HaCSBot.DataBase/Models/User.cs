using HaCSBot.DataBase.Enums;

namespace HaCSBot.DataBase.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Phone { get; set; }
        public Roles Roles { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

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
                Roles = user.Roles,
                CreatedDate = user.CreatedDate,
            };
        }
    }
}

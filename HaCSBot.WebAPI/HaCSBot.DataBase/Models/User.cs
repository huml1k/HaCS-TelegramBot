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
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public List<Apartment> Apartments { get; set; }

        // новые свойства
		public long? TelegramId { get; set; }        // null, если ещё не привязан к Telegram
		public bool IsAuthorizedInBot { get; set; }  // флаг: вошёл ли через /start
		public DateTime? LastAuthorizationDate { get; set; }

    }
}

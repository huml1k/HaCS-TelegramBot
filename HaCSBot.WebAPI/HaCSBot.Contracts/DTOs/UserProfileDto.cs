using HaCSBot.DataBase.Enums;

namespace HaCSBot.Contracts.DTOs
{
	public class UserProfileDto
	{
		public Guid Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public Roles Role { get; set; }
		public List<string> Apartments { get; set; } = new List<string>();
	}
}

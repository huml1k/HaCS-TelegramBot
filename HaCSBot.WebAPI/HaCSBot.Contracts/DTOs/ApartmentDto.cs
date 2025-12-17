namespace HaCSBot.Contracts.DTOs
{
	public class ApartmentDto
	{
		public Guid Id { get; set; }
		public Guid? UserId { get; set; }
        public string Number { get; set; } = string.Empty;
		public string? OwnerName { get; set; }
		public string BuildingAddress { get; set; } = string.Empty;
		public Guid BuildingId { get; set; }
	}
}

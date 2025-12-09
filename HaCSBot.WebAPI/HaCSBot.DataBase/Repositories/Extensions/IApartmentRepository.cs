using HaCSBot.DataBase.Models;

namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IApartmentRepository 
    {
		public Task<Apartment> GetByIdAsync(Guid id);
		public Task<Apartment> GetByNumberAndBuildingIdAsync(string apartmentNumber, Guid buildingId);
		public Task<List<Apartment>> GetByUserIdAsync(Guid userId); // все квартиры конкретного жильца
		public Task<Apartment> GetWithUserAndBuildingAsync(Guid apartmentId); // подгружает User + Building (самый частый запрос)
		public Task<List<Apartment>> GetApartmentsByBuildingIdAsync(Guid buildingId); // все квартиры в доме
		public Task AddAsync(Apartment apartment);
		public Task UpdateAsync(Apartment apartment);
		public Task DeleteAsync(Guid id);
		public Task<bool> ExistsInBuildingAsync(string apartmentNumber, Guid buildingId); // проверка, есть ли уже такая квартира в доме
		public Task<List<Apartment>> GetUnoccupiedApartmentsAsync(Guid buildingId); // квартиры в доме, у которых UserId == null (для админа — кто ещё не зарегистрировался)

	}
}

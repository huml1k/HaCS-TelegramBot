using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class ApartmentService : IApartmentService
    {
        private readonly IApartmentRepository _repository;

        public ApartmentService(IApartmentRepository repository)
        {
            _repository = repository;
        }

        public Task AddAsync(Apartment apartment)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsInBuildingAsync(string apartmentNumber, Guid buildingId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Apartment>> GetApartmentsByBuildingIdAsync(Guid buildingId)
        {
            throw new NotImplementedException();
        }

        public Task<Apartment> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Apartment> GetByNumberAndBuildingIdAsync(string apartmentNumber, Guid buildingId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Apartment>> GetByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Apartment>> GetUnoccupiedApartmentsAsync(Guid buildingId)
        {
            throw new NotImplementedException();
        }

        public Task<Apartment> GetWithUserAndBuildingAsync(Guid apartmentId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Apartment apartment)
        {
            throw new NotImplementedException();
        }
    }
}

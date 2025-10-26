using AutoMapper;
using HaCSBot.Contracts.Contracts;
using HaCSBot.DataBase.Enums;
using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using HaCSBot.Services.Services.Extensions;

namespace HaCSBot.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.Delete(id);
        }

        public async Task<int> GetCountUsers()
        {
            return await _userRepository.GetCountValues();
        }

        public async Task<DateTime> GetEarliestRegistrationDate()
        {
            return await _userRepository.GetEarliestRegistrationDate();
        }

        public async Task<DateTime> GetLatestRegistrationDate()
        {
            return await _userRepository.GetLatestRegistrationDate();
        }

        public async Task<IEnumerable<User>> GetSortedListUsers()
        {
            return await _userRepository.GetSortedList();
        }

        public async Task<IEnumerable<User>> GetUsersByDateRangeAsync(DateTime? start, DateTime? end)
        {
            return await _userRepository.GetUsersByDateRangeAsync(start, end);
        }

        public async Task<IEnumerable<User>> GetUsersBySex(Sex sex)
        {
            return await _userRepository.GetUserBySex(sex);
        }



        public async Task<bool> RegisterAsync(UserRegistrationContract request)
        {
            if (request == null)
            {
                return false;
            }
            var resultRegistration = _mapper.Map<User>(request);
            await _userRepository.Create(resultRegistration);
            return true;
        }
    }
}

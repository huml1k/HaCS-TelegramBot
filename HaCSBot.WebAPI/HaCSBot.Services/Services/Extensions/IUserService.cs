using HaCSBot.Contracts.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaCSBot.Services.Services.Extensions
{
    public interface IUserService
    {
        public Task<bool> RegisterAsync(UserRegistrationContract request);
        //public Task<bool> LoginAsync(LoginContract request);
        //public Task UpdateUserAsync(string username, UserUpdateContact request);
        public Task DeleteUserAsync(Guid id);
        //public Task<IEnumerable<User>> GetUsersByDateRangeAsync(DateTime? start, DateTime? end);
        //public Task<IEnumerable<User>> GetSortedListUsers();
        //public Task<IEnumerable<User>> GetUsersBySex(Sex sex);
        public Task<int> GetCountUsers();
        public Task<DateTime> GetEarliestRegistrationDate();
        public Task<DateTime> GetLatestRegistrationDate();
    }
}

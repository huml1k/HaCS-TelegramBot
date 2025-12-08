using HaCSBot.Contracts.Contracts;
using HaCSBot.DataBase.Models;
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
        public Task<User?> FindByPersonalDataAsync(string firstName, string lastName, string phone);
		public Task DeleteUserAsync(Guid id);
		Task UpdateUserAsync(User user);
		Task<User?> GetByTelegramIdAsync(long telegramId);
	}
}

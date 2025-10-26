using HaCSBot.DataBase.Models;
using HaCSBot.DataBase.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HaCSBot.DataBase.Repositories
{
    public class UserRepository : IGenericRepository<User>
    {
        private readonly MyApplicationDbContext _dbContext;
        public UserRepository(MyApplicationDbContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task Create(User user)
        {
            var userResult = user.CreateUser(user);

            await _dbContext.AddAsync(userResult);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            await _dbContext.Users
                .Where(x => x.Id == id)
                .AsNoTracking()
                .ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            var result = _dbContext.Users.ToListAsync();
            return await result;
        }

        public async Task<int> GetCountValues()
        {
            var result = _dbContext.Users.Count();
            return result;
        }

        public async Task<User> GetCurrentValue(Guid id)
        {
            var result = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return result;
        }

        public async Task<DateTime> GetEarliestRegistrationDate()
        {
            var result = await _dbContext.Users
                .AsNoTracking()
                .MinAsync(x => x.CreatedDate);

            return result;
        }

        public async Task<DateTime> GetLatestRegistrationDate()
        {
            var result = await _dbContext.Users
                .AsNoTracking()
                .MaxAsync(x => x.CreatedDate);

            return result;
        }

        public async Task<IEnumerable<User>> GetSortedList()
        {
            var result = await _dbContext.Users
                .AsNoTracking()
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();

            return result;
        }

        public async Task Update(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}

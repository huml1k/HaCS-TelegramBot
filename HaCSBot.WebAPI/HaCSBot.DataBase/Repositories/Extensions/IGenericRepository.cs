namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IGenericRepository<T> where T : class
    {
        public Task Create(T user);
        public Task Update(T user);
        public Task Delete(Guid id);
        public Task<T> GetCurrentValue(Guid id);
        public Task<IEnumerable<T>> GetAll();
        public Task<int> GetCountValues();
        public Task<IEnumerable<T>> GetSortedList();
    }
}

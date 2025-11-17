namespace HaCSBot.DataBase.Repositories.Extensions
{
    public interface IGenericRepository<T> where T : class
    {
        public Task Create(T entity);
        public Task Update(T entity);
        public Task Delete(Guid id);
        public Task<T> GetById(Guid id);
        public Task<IEnumerable<T>> GetAll();
    }
}

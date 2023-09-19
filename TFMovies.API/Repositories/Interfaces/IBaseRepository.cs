namespace TFMovies.API.Repositories.Interfaces;

public interface IBaseRepository<T>
    where T : class
{
    public Task<T?> GetByIdAsync(string id);
    public Task<T?> GetByKeyValuesAsync(object[] keyValues);
    public Task<IEnumerable<T>> GetAllAsync();
    public Task<T> CreateAsync(T entity);
    public Task CreateRangeAsync(IEnumerable<T> entities);
    public Task<T> UpdateAsync(T entity);
    public Task DeleteAsync(T entity);
    public Task DeleteByIdAsync(string id);
    public Task DeleteRangeAsync(IEnumerable<T> entities);
    public Task SaveChangesAsync();
}

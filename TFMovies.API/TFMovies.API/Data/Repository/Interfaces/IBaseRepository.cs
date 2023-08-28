namespace TFMovies.API.Data.Repository.Interfaces;

public interface IBaseRepository<T>
    where T : class
{
    public ValueTask<T> CreateAsync(T entity);
    public ValueTask<T> UpdateAsync(T entity);
    public Task SaveChangesAsync();
}

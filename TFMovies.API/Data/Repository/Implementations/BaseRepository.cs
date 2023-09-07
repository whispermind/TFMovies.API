using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public abstract class BaseRepository<T> : IBaseRepository<T>
    where T : class
{
    protected readonly DbContext _context;
    protected DbSet<T> _entities;

    public BaseRepository(DataContext context)
    {
        _context = context;
        _entities = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _entities.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _entities.FindAsync(id);
    }

    public async Task<T?> GetByKeyValuesAsync(object[] keyValues)
    {
        return await _entities.FindAsync(keyValues);
    }

    public async Task<T> CreateAsync(T entity)
    {
        await _entities.AddAsync(entity);
        await SaveChangesAsync();

        return entity;
    }

    public async Task CreateRangeAsync(IEnumerable<T> entities)
    {
        await _entities.AddRangeAsync(entities);
        await SaveChangesAsync();
    }

    public async Task<T> UpdateAsync(T updatedEntity)
    {
        _entities.Update(updatedEntity);
        await SaveChangesAsync();

        return updatedEntity;
    }

    public async Task DeleteAsync(T entity)
    {
        _entities.Remove(entity);
        await SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(string id)
    {
        var entity = await _entities.FindAsync(id);
        if (entity != null)
        {
            _entities.Remove(entity);
            await SaveChangesAsync();
        }
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

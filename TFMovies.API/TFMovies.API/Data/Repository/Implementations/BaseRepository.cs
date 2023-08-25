using Microsoft.EntityFrameworkCore;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class BaseRepository<T> : IBaseRepository<T>
    where T : class
{
    protected readonly DbContext _context;
    protected DbSet<T> _entities;

    public BaseRepository(DataContext context)
    {
        _context = context;
        _entities = _context.Set<T>();
    }

    public async ValueTask<T> CreateAsync(T entity)
    {
        await _entities.AddAsync(entity);
        await SaveChangesAsync();

        return entity;
    }   

    public async ValueTask<T> UpdateAsync(T updatedEntity)
    {
        _entities.Update(updatedEntity);
        await SaveChangesAsync();

        return updatedEntity;
    } 
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

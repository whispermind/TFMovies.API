using Microsoft.EntityFrameworkCore;
using System.Net;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Exceptions;
using TFMovies.API.Extensions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public abstract class BaseRepository<T> : IBaseRepository<T>
    where T : BaseModel
{
    protected readonly DbContext _context;
    protected DbSet<T> _entities;
    protected virtual IEnumerable<string>? SearchColumns { get; }
    protected BaseRepository(DataContext context)
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

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        if (entities.Any())
        {
            _entities.RemoveRange(entities);
            await SaveChangesAsync();
        }
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public IQueryable<T> Query()
    {
        return _entities.AsNoTracking();
    }

    public async Task<PagedResult<T>> GetPagedDataAsync(PagingSortDto<T> dto, IQueryable<T>? queryOverride = null)
    {
        var query = queryOverride ?? _entities;

        return await query.GetPagedDataAsync(dto);
    }

    public IQueryable<T> SearchByTerms(IEnumerable<string> terms)
    {
        if (SearchColumns == null || !SearchColumns.Any())
        {
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.SearchColumnsNotDefined);
        }

        return _entities
            .SearchByTerms(SearchColumns, terms);
    }

    public async Task<IEnumerable<string>> GetMatchingIdsAsync(IEnumerable<string> terms)
    {
        var query = SearchByTerms(terms);

        return await query
            .Select(t => t.Id)
            .ToListAsync();
    }
}

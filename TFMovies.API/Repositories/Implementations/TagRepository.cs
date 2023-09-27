using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    protected override IEnumerable<string> SearchColumns => new[] { "Name" };
    public TagRepository(DataContext context) : base(context)
    { }

    public async Task<Tag?> FindByNameAsync(string name)
    {
        var result = await _entities
             .FirstOrDefaultAsync(item =>
                 item.Name == name);

        return result;
    }

    public async Task<IEnumerable<Tag>> FindByNamesAsync(IEnumerable<string> tagNames)
    {
        var result = await _entities
            .Where(t => tagNames.Contains(t.Name))
            .ToListAsync();
        
        return result;
    }

    public async Task<IEnumerable<Tag>> GetTagsAsync(int? limit, string? sort, string? order)
    {
        var actualLimit = limit ?? 10;

        IQueryable<Tag> query = _entities; 

        Expression<Func<Tag, object>> sortSelector;
        
        switch (sort)
        {
            case SortOptions.Rated:
                sortSelector = t => t.PostTags.Count();
                break;
            default:
                sortSelector = t => t.Name;
                break;
        }

        if (string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase))
        {
            query = query.OrderBy(sortSelector);
        }
        else
        {
            query = query.OrderByDescending(sortSelector);
        }

        var result = await query
                .Take(actualLimit)
                .ToListAsync();

        return result;
    }   
}

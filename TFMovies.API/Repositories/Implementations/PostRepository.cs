using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
using TFMovies.API.Filters;
using TFMovies.API.Models.Dto;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(DataContext context) : base(context)
    { }

    public async Task<Post?> GetFullByIdAsync(string id)
    {
        var result =  await _entities
            .Include(p => p.User)
            .Include(p => p.Theme)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostComments)
                .ThenInclude(pc => pc.User)
            .Include(p => p.PostLikes)
            .FirstOrDefaultAsync(p => p.Id == id);

        return result;
    }

    public async Task<IEnumerable<Post>> GetOthersAsync(string excludeId, string authorId, int limit)
    {        
        var result = await _entities
            .Where(p => p.UserId == authorId && p.Id != excludeId)            
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return result;
    }

    public async Task<PagedResult<Post>> GetAllPagingAsync(PaginationFilter paging, string? sort, string? themeId)
    {
        IQueryable<Post> query = Query()
            .Include(p => p.User)
            .Include(p => p.Theme)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostLikes);

        if (!string.IsNullOrEmpty(themeId))
        {
            query = query.Where(p => p.ThemeId == themeId);
        }

        Expression<Func<Post, object>> sortSelector;
        switch (sort)
        {
            case "rated":
                sortSelector = p => p.PostLikes.Count;
                break;
            default:
                sortSelector = p => p.CreatedAt; // default
                break;
        }

        var pagedResult = await query.GetPagedDataAsync(paging, sortSelector, "desc");

        return pagedResult;
    }    
}

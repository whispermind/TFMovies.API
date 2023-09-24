using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
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

    public async Task<PagedResult<Post>> GetAllPagingAsync(PaginationSortFilterParams model)
    {
        IQueryable<Post> query = Query();

        if (!string.IsNullOrEmpty(model.ThemeId))
        {
            query = query.Where(p => p.ThemeId == model.ThemeId);
        }

        query = query.Include(p => p.User)
            .Include(p => p.User)
            .Include(p => p.Theme)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostLikes)
            .AsSplitQuery();


        Expression<Func<Post, object>> sortSelector;
        switch (model.Sort)
        {
            case SortOptions.Rated:
                sortSelector = p => p.PostLikes.Count;
                break;
            default:
                sortSelector = p => p.CreatedAt; // default
                break;
        }

        var pagingSortDto = new PaginationSortDto<Post>
        {
            Page = model.Page,
            Limit = model.Limit,
            SortSelector = sortSelector
        };

        var pagedResult = await query.GetPagedDataAsync(pagingSortDto);

        return pagedResult;
    }

    public async Task<PagedResult<Post>> GetByIdsPagingAsync(IEnumerable<string> postIds, PaginationSortFilterParams model)
    {
        IQueryable<Post> query = _entities
            .Where(p => postIds.Contains(p.Id))
            .Include(p => p.User)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostLikes);

        Expression<Func<Post, object>> sortSelector = p => p.LikeCount;

        var pagingSortDto = new PaginationSortDto<Post>
        {
            Page = model.Page,
            Limit = model.Limit,
            SortSelector = sortSelector
        };

        return await query.GetPagedDataAsync(pagingSortDto);
    }
}

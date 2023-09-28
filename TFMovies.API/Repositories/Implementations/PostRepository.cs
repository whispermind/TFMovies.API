using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Repositories.Interfaces;


namespace TFMovies.API.Repositories.Implementations;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    protected override IEnumerable<string> SearchColumns => new[] { "Title", "HtmlContent" };
    public PostRepository(DataContext context) : base(context)
    { }

    public async Task<Post?> GetFullByIdAsync(string id)
    {
        return await _entities
            .Include(p => p.User)
            .Include(p => p.Theme)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostComments)
                .ThenInclude(pc => pc.User)
            .Include(p => p.PostLikes)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetOthersAsync(string excludeId, string authorId, int limit)
    {
        IQueryable<Post> query = Query()
            .Where(p => p.UserId == authorId && p.Id != excludeId);

        query = query
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .OrderByDescending(p => p.CreatedAt);

        if (limit > 0)
        {
            query = query.Take(limit);
        }
        
        return await query.ToListAsync();
    }

    public async Task<PagedResult<Post>> GetAllPagingAsync(PagingSortParams pagingSortModel, PostsFilterParams filterModel, PostsQueryDto dto)
    {
        var query = Query();       

        // Filter by Theme
        if (!string.IsNullOrEmpty(filterModel.ThemeId))
        {
            query = query.Where(p => p.ThemeId == filterModel.ThemeId);

            return await FetchPagedResultsAsync(query, pagingSortModel);
        }

        // Search
        if (dto.Query != null && dto.Query.Any())
        {
            query = query.SearchByTerms(SearchColumns, dto.Query);
        }

        if (dto.MatchingTagIdsQuery != null && dto.MatchingTagIdsQuery.Any())
        {
            query = query.Where(p => p.PostTags.Any(pt => dto.MatchingTagIdsQuery.Contains(pt.TagId)));
        }

        if (dto.MatchingCommentIdsQuery != null && dto.MatchingCommentIdsQuery.Any())
        {
            query = query.Where(p => p.PostComments.Any(pc => dto.MatchingCommentIdsQuery.Contains(pc.Id)));
        }

        return await FetchPagedResultsAsync(query, pagingSortModel);
    }

    public async Task<PagedResult<Post>> GetByIdsPagingAsync(IEnumerable<string> postIds, PagingSortParams model)
    {
        var query = _entities
            .Where(p => postIds.Contains(p.Id));

        return await FetchPagedResultsAsync(query, model, p => p.LikeCount);
    }    

    //helpers
    private async Task<PagedResult<Post>> FetchPagedResultsAsync(
        IQueryable<Post> query,
        PagingSortParams pagingSortModel,
        Expression<Func<Post, object>>? defaultSortSelector = null)
    {
        query = IncludeCommonPostEntities(query);

        var sortSelector = defaultSortSelector ?? GetSortSelector(pagingSortModel);

        var pagingSortDto = pagingSortModel.ToPagingDto(sortSelector);

        return await query.GetPagedDataAsync(pagingSortDto);
    }

    private IQueryable<Post> IncludeCommonPostEntities(IQueryable<Post> query)
    {
        return query
            .Include(p => p.User)
            .Include(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Include(p => p.PostLikes)
            .AsSplitQuery()
            .AsNoTracking();
    }

    private Expression<Func<Post, object>> GetSortSelector(PagingSortParams model)
    {
        return model.Sort switch
        {
            SortOptions.Rated => p => p.LikeCount,
            _ => p => p.CreatedAt // default
        };
    }
}

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
    protected override IEnumerable<string> SearchColumns => new[] { "Title", "HtmlContent" };
    public PostRepository(DataContext context) : base(context)
    { }

    public async Task<Post?> GetFullByIdAsync(string id)
    {
        return await _entities
            .Include(p => p.User)
            .Include(p => p.Theme)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.PostComments).ThenInclude(pc => pc.User)
            .Include(p => p.PostLikes)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Post>> GetOthersAsync(string excludeId, string authorId, int limit)
    {
        return await _entities
            .Where(p => p.UserId == authorId && p.Id != excludeId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<PagedResult<Post>> GetAllPagingAsync(PagingSortFilterParams model)
    {
        var query = Query();

        Expression<Func<Post, bool>>? filterPredicate = null;

        if (!string.IsNullOrEmpty(model.ThemeId))
        {
            filterPredicate = p => p.ThemeId == model.ThemeId;
        }

        return await FetchPagedResultsAsync(query, model, filterPredicate: filterPredicate);
    }

    public async Task<PagedResult<Post>> GetByIdsPagingAsync(IEnumerable<string> postIds, PagingSortFilterParams model)
    {
        var query = _entities
            .Where(p => postIds.Contains(p.Id));

        return await FetchPagedResultsAsync(query, model, p => p.LikeCount);
    }

    public async Task<PagedResult<Post>> SearchWihPagingAsync(IEnumerable<string> terms, PagingSortFilterParams model)
    {
        var query = SearchByTerms(terms);

        return await FetchPagedResultsAsync(query, model);
    }

    public async Task<PagedResult<Post>> SearchByTagIdsWihPagingAsync(IEnumerable<string> tagIds, PagingSortFilterParams model)
    {
        var query = _entities
            .Where(p => p.PostTags
                .Any(pt => tagIds.Contains(pt.TagId)));

        return await FetchPagedResultsAsync(query, model);
    }

    public async Task<PagedResult<Post>> SearchByCommentIdsWihPagingAsync(IEnumerable<string> postCommentIds, PagingSortFilterParams model)
    {
        var query = _entities
            .Where(p => p.PostComments
                .Any(pc => postCommentIds.Contains(pc.Id)));

        return await FetchPagedResultsAsync(query, model);
    }

    private async Task<PagedResult<Post>> FetchPagedResultsAsync(
        IQueryable<Post> query,
        PagingSortFilterParams model,
        Expression<Func<Post, object>>? defaultSortSelector = null,
        Expression<Func<Post, bool>>? filterPredicate = null)
    {
        query = IncludeCommonPostEntities(query);

        var sortSelector = defaultSortSelector ?? GetSortSelector(model);

        var pagingSortFilterDto = model.ToPagingDto(sortSelector, filterPredicate);

        return await query.GetPagedDataAsync(pagingSortFilterDto);
    }

    private IQueryable<Post> IncludeCommonPostEntities(IQueryable<Post> query)
    {
        return query
            .Include(p => p.User)
            .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
            .Include(p => p.PostLikes)
            .AsSplitQuery()
            .AsNoTracking();
    }

    private Expression<Func<Post, object>> GetSortSelector(PagingSortFilterParams model)
    {
        return model.Sort switch
        {
            SortOptions.Rated => p => p.LikeCount,
            _ => p => p.CreatedAt // default
        };
    }
}

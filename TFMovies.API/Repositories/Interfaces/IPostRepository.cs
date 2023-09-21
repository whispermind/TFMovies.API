using TFMovies.API.Data.Entities;
using TFMovies.API.Filters;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Repositories.Interfaces;

public interface IPostRepository : IBaseRepository<Post>
{
    public Task<Post?> GetFullByIdAsync(string id);
    public Task<IEnumerable<Post>> GetOthersAsync(string excludeId, string authorId, int limit);
    public Task<PagedResult<Post>> GetAllPagingAsync(PaginationFilter paging, string? sort, string? themeId);
}

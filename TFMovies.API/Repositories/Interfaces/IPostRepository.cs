using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Repositories.Interfaces;

public interface IPostRepository : IBaseRepository<Post>
{
    public Task<Post?> GetFullByIdAsync(string id);
    public Task<IEnumerable<Post>> GetOthersAsync(string excludeId, string authorId, int limit);
    public Task<PagedResult<Post>> GetAllPagingAsync(PagingSortParams pagingSortModel, PostsFilterParams filterModel, PostsQueryDto dto);
    public Task<PagedResult<Post>> GetByIdsPagingAsync(IEnumerable<string> postIds, PagingSortParams model);    
}

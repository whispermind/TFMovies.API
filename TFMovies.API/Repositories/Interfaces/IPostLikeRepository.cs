using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Repositories.Interfaces;

public interface IPostLikeRepository : IBaseRepository<PostLike>
{
    public Task<List<string>> GetLikedPostIdsByUserIdAsync(string userId);
    public Task<PostLike?> GetPostLikeAsync(string postId, string userId);
    public Task<IEnumerable<UserPostLikeCountsDto>> GetUserIdsByPostLikeCountsAsync(int limit, string? order);
}

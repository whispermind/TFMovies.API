using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Entitiesl;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Repositories.Interfaces;

public interface IPostLikeRepository : IBaseRepository<PostLike>
{
    public Task<IEnumerable<Post>> GetUserFavoritePostsAsync(string userId);
    public Task<PostLike?> GetPostLikeAsync(string postId, string userId);
    public Task<IEnumerable<AuthorLikeCountDto>> GetAuthorIdsByLikeCountsAsync(int limit);
}

using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Entitiesl;

namespace TFMovies.API.Repositories.Interfaces;

public interface IPostLikeRepository : IBaseRepository<PostLike>
{
    public Task<IEnumerable<Post>> GetUserFavoritePostsAsync(string userId);
    public Task<PostLike?> GetPostLikeAsync(string postId, string userId);
}

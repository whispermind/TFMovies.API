using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Entitiesl;
using TFMovies.API.Data;
using TFMovies.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TFMovies.API.Repositories.Implementations;

public class PostLikeRepository : BaseRepository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(DataContext context) : base(context)
    { }

    public async Task<IEnumerable<Post>> GetUserFavoritePostsAsync(string userId)
    {
        var result = await _entities
            .Where(e => e.UserId == userId)
            .Select(e => e.Post)
            .ToListAsync();

        return result;
    }

}

using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Entitiesl;
using TFMovies.API.Data;
using TFMovies.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Repositories.Implementations;

public class PostLikeRepository : BaseRepository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(DataContext context) : base(context)
    { }

    public async Task<IEnumerable<Post>> GetUserFavoritePostsAsync(string userId)
    {
        var result = await _entities
            .Where(e => e.UserId == userId)
            .Include(e => e.Post)
                .ThenInclude(p => p.User)
            .Include(e => e.Post)
            .ThenInclude(p => p.PostTags)
                .ThenInclude(pt => pt.Tag)
            .Select(e => e.Post)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return result;
    }

    public async Task<PostLike?> GetPostLikeAsync(string postId, string userId)
    {
        var result = await _entities
            .Where(pl => pl.PostId == postId && pl.UserId == userId)
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<IEnumerable<AuthorLikeCountDto>> GetAuthorIdsByLikeCountsAsync(int limit)
    {
        var result = await _entities
            .Include(pl => pl.Post)
            .Where(pl => pl.Post != null)
            .GroupBy(pl => pl.Post.UserId)
            .Select(g => new AuthorLikeCountDto
            {
                AuthorId = g.Key,
                LikeCount = g.Count()
            })
            .OrderByDescending(g => g.LikeCount)
            .Take(limit)
            .ToListAsync();

        return result;
    }
}

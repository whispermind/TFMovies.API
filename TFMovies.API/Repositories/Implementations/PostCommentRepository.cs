using TFMovies.API.Data.Entities;
using TFMovies.API.Data;
using TFMovies.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TFMovies.API.Repositories.Implementations;

public class PostCommentRepository : BaseRepository<PostComment>, IPostCommentRepository
{
    protected override IEnumerable<string> SearchColumns => new[] { "Content" };
    public PostCommentRepository(DataContext context) : base(context)
    { }
    public async Task<IEnumerable<PostComment>> GetAllByPostIdAsync(string postId)
    {
        var result = await Query()
            .Where(e => e.PostId == postId)
            .Include(e => e.User)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return result;
    }
}

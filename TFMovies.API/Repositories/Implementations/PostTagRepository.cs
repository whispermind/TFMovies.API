using Microsoft.EntityFrameworkCore;
using System.Linq;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class PostTagRepository : BaseRepository<PostTag>, IPostTagRepository
{
    public PostTagRepository(DataContext context) : base(context)
    { }

    public async Task<List<PostTag>?> FindByPostIdAsync(string postId)
    {
        var result = await _entities
                   .Where(item => item.PostId == postId)
                   .ToListAsync();
       
        return result;
    }
}

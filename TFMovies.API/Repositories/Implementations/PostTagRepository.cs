using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class PostTagRepository : BaseRepository<PostTag>, IPostTagRepository
{
    public PostTagRepository(DataContext context) : base(context)
    { }
}

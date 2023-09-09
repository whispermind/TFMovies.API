using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(DataContext context) : base(context)
    { }
}

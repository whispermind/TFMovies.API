using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class PostRepository : BaseRepository<Post>, IPostRepository
{
    public PostRepository(DataContext context) : base(context)
    { }
}

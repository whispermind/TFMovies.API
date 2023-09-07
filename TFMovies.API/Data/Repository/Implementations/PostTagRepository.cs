using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class PostTagRepository : BaseRepository<PostTag>, IPostTagRepository
{
    public PostTagRepository(DataContext context) : base(context)
    { }
}

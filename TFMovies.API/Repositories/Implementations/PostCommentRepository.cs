using TFMovies.API.Data.Entities;
using TFMovies.API.Data;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class PostCommentRepository : BaseRepository<PostComment>, IPostCommentRepository
{
    public PostCommentRepository(DataContext context) : base(context)
    { }
}

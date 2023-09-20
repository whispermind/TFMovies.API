using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Repositories.Interfaces;

public interface IPostCommentRepository : IBaseRepository<PostComment>
{
    public Task<IEnumerable<PostComment>> GetAllByPostIdAsync(string postId);
}

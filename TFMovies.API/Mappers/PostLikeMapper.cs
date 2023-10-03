using TFMovies.API.Data.Entities;

namespace TFMovies.API.Mappers;

public static class PostLikeMapper
{
    public static PostLike ToCreateEntity(string postId, User currentUser)
    {
        var result = new PostLike
        {
            PostId = postId,
            UserId = currentUser.Id
        };

        return result;
    }
}

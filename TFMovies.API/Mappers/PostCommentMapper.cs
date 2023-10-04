using Microsoft.Extensions.Hosting;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Mappers;

public static class PostCommentMapper
{
    public static CommentDetailDto ToCommentDetailDto(PostComment comment)
    {
        var result = new CommentDetailDto
        {
            Id = comment.Id,
            AuthorId = comment.User.Id,
            Author = comment.User.IsDeleted ? UserConstants.DeletedUserName : comment.User.Nickname,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };

        return result;
    }

    public static PostComment ToCreateEntity (string postId, PostAddCommentRequest model, User currentUser)
    {
        var result = new PostComment
        {
            PostId = postId,
            UserId = currentUser.Id,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow
        };

        return result;
    }

    public static PostAddCommentResponse ToPostAddCommentResponse(PostComment comment)
    {
        var result = new PostAddCommentResponse
        {
            PostId = comment.PostId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            Author = comment.User.Nickname
        };

        return result;
    }
}

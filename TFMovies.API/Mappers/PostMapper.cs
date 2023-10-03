using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Mappers;

public class PostMapper
{
    public static PostShortInfoDto ToPostShortInfoDto(Post post, User? currentUser)
    {
        var result = new PostShortInfoDto
        {
            Id = post.Id,
            CoverImageUrl = post.CoverImageUrl,
            Title = post.Title,
            CreatedAt = post.CreatedAt,
            AuthorId = post.User.Id,
            Author = post.User.IsDeleted ? UserConstants.DeletedUserName : post.User.Nickname,
            IsLiked = currentUser != null && post.PostLikes != null && post.PostLikes.Any(pl => pl.UserId == currentUser.Id),
            LikesCount = post.LikeCount,
            Tags = post.ToTagDtos()
        };

        return result;
    }

    public static Post ToCreateEntity(PostCreateRequest model, User currentUser)
    {
        var result = new Post
        {
            UserId = currentUser.Id,
            ThemeId = model.ThemeId,
            Title = model.Title,
            HtmlContent = model.HtmlContent,
            CoverImageUrl = model.CoverImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return result;
    }

    public static void ToUpdateEntity(Post postDb, PostUpdateRequest requestModel, Theme theme)
    {
        postDb.CoverImageUrl = requestModel.CoverImageUrl;
        postDb.ThemeId = theme.Id;
        postDb.Title = requestModel.Title;
        postDb.HtmlContent = requestModel.HtmlContent;
        postDb.UpdatedAt = DateTime.UtcNow;
    }    
}

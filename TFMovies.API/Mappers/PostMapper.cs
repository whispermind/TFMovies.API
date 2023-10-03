using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Mappers;

public static class PostMapper
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

    public static PostsQueryDto ToPostsQueryDto(IEnumerable<string> termsQuery, IEnumerable<string> matchedTagIds, IEnumerable<string> matchedCommentIds)
    {
        var result =  new PostsQueryDto
        {
            Query = termsQuery,
            MatchingTagIdsQuery = matchedTagIds,
            MatchingCommentIdsQuery = matchedCommentIds
        };

        return result;
    }
    public static PostByAuthorDto ToPostByAuthorDto(Post post)
    {
        var result = new PostByAuthorDto
        {
            Id = post.Id,
            Title = post.Title,
            CreatedAt = post.CreatedAt,
            Tags = post.ToTagDtos()
        };

        return result;
    }

    public static PostGetByIdResponse ToPostGetByIdResponse(
        Post post, 
        User? currentUser,
        ThemeDto theme,
        IEnumerable<PostByAuthorDto>? otherPostsDtos,
        IEnumerable<CommentDetailDto>? commentDetails)
    {
        var result = new PostGetByIdResponse
        {
            Id = post.Id,
            CoverImageUrl = post.CoverImageUrl,
            Title = post.Title,
            HtmlContent = post.HtmlContent,
            CreatedAt = post.CreatedAt,
            AuthorId = post.UserId,
            Author = post.User.Nickname,
            IsLiked = currentUser != null && post.PostLikes != null && post.PostLikes.Any(pl => pl.UserId == currentUser.Id),
            LikesCount = post.LikeCount,
            CommentsCount = post.PostComments?.Count ?? 0,
            Theme = theme,
            Tags = post.ToTagDtos(),
            Comments = commentDetails,
            PostsByAuthor = otherPostsDtos
        };

        return result;
    }
}

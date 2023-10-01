using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Mappers;

public class PostMapper
{
    public static PostsPaginatedResponse ToPostsPaginatedResponse(PagedResult<Post> pagedPosts, User? currentUser)
    {
        var data = pagedPosts.Data.Select(p => new PostShortInfoDto
        {
            Id = p.Id,
            CoverImageUrl = p.CoverImageUrl,
            Title = p.Title,
            CreatedAt = p.CreatedAt,
            AuthorId = p.User.Id,
            Author = p.User.Nickname,
            IsLiked = (currentUser == null || p.PostLikes == null) ? false : p.PostLikes.Any(pl => pl.UserId == currentUser.Id),
            LikesCount = p.LikeCount,
            Tags = p.ToTagDtos()
        }).ToList();

        var result = new PostsPaginatedResponse
        {
            Page = pagedPosts.Page,
            Limit = pagedPosts.Limit,
            TotalPages = pagedPosts.TotalPages,
            TotalRecords = pagedPosts.TotalRecords,
            Data = data
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

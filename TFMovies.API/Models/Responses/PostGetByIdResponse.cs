using TFMovies.API.Models.Dto;

namespace TFMovies.API.Models.Responses;

public class PostGetByIdResponse
{
    public string Id { get; set; }
    public string CoverImageUrl { get; set; }
    public string Title { get; set; }
    public string Theme { get; set; }
    public string HtmlContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorId { get; set; }
    public string Author { get; set; }
    public bool IsLiked { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }

    public IEnumerable<string>? Tags { get; set; }
    public IEnumerable<CommentDetailDto>? Comments { get; set; }
    public IEnumerable<PostByAuthorDto>? PostsByAuthor { get; set; }
}

  

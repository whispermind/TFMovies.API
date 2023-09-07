using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Extensions.Attributes;

namespace TFMovies.API.Models.Requests;

public class CreatePostRequest
{
    [Required]
    public string CoverImageUrl { get; set; }

    [Required]
    public string Theme { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string HtmlContent { get; set; }

    [MaxItemCount(5, ErrorMessage = ErrorMessages.MaxTagsItemError)]
    public List<string> Tags { get; set; }
}

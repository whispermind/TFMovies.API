using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Extensions.Attributes;

namespace TFMovies.API.Models.Requests;

public class PostUpdateRequest
{
    [Required]
    public string CoverImageUrl { get; set; }

    [Required]
    public string ThemeId { get; set; }

    [Required]
    [MaxLength(250, ErrorMessage = ErrorMessages.TitleMaxLengthError)]
    public string Title { get; set; }

    [Required]
    [MaxLength(250000, ErrorMessage = ErrorMessages.HtmlContentMaxLengthError)]
    public string HtmlContent { get; set; }

    [MaxItemCount(5, ErrorMessage = ErrorMessages.MaxTagsItemError)]
    public List<string> Tags { get; set; }
}

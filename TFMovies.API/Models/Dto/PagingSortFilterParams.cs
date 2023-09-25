using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Models.Dto;

public class PagingSortFilterParams
{
    [Range(1, LimitValues.MaxValue)]
    public int? Page { get; set; }

    [Range(1, LimitValues.MaxValue)]
    public int? Limit { get; set; }
    public string? Sort { get; set; }
    public string? Order { get; set; }
    public string? ThemeId { get; set; }
}

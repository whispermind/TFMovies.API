using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Models.Requests;

public class SortFilterRequest
{
    [Range(1, LimitValues.MaxValue)]
    public int Limit { get; set; } = 1;
    public string Sort { get; set; } = SortOptions.Rated;
    public string Order { get; set; } = "desc";
}

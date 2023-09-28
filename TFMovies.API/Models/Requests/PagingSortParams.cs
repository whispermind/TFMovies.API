using System.ComponentModel.DataAnnotations;
using TFMovies.API.Common.Constants;

namespace TFMovies.API.Models.Requests;

public class PagingSortParams
{
    [Range(1, LimitValues.MaxValue)]
    public int? Page { get; set; }

    [Range(1, LimitValues.MaxValue)]
    public int? Limit { get; set; }
    public string? Sort { get; set; }
    public string? Order { get; set; }    
}

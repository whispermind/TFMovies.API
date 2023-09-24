using System.Linq.Expressions;

namespace TFMovies.API.Models.Dto;

public class PaginationSortDto<T>
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public Expression<Func<T, object>>? SortSelector { get; set; }
    public string? Order { get; set; }
}

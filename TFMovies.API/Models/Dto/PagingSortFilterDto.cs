using System.Linq.Expressions;

namespace TFMovies.API.Models.Dto;

public class PagingSortFilterDto<T>
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
    public string? Order { get; set; }
    public Expression<Func<T, object>>? SortSelector { get; set; }
    public Expression<Func<T, bool>>? FilterPredicate { get; set; }
}

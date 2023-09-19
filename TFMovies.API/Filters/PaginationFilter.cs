using TFMovies.API.Common.Constants;

namespace TFMovies.API.Filters;
public class PaginationFilter
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public PaginationFilter()
    {
        Page = DefaultPaginationValues.DefaultPageNumber;
        Limit = DefaultPaginationValues.MaxLimit;
    }
    public PaginationFilter(int page, int limit)
    {
        Page = page < DefaultPaginationValues.DefaultPageNumber ? DefaultPaginationValues.DefaultPageNumber : page;
        Limit = limit > DefaultPaginationValues.MaxLimit ? DefaultPaginationValues.MaxLimit : limit;
    }
}

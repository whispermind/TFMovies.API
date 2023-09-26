namespace TFMovies.API.Models.Dto;

public class PagedResult<T>
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public IEnumerable<T> Data { get; set; }
}

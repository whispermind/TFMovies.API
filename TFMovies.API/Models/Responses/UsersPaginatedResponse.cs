using TFMovies.API.Models.Dto;

namespace TFMovies.API.Models.Responses;

public class UsersPaginatedResponse
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public IEnumerable<UserShortInfoDto>? Data { get; set; }
}

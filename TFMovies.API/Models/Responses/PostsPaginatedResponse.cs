using TFMovies.API.Models.Dto;

namespace TFMovies.API.Models.Responses;

public class PostsPaginatedResponse
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }   
    public IEnumerable<PostShortInfoDto>? Data { get; set; }
}    

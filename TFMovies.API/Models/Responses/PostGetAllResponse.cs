namespace TFMovies.API.Models.Responses
{
    public class PostGetAllResponse
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public string? Theme { get; set; }
        public string? Sort { get; set; }

        public IEnumerable<PostShortInfoDto>? Data { get; set; }

    }
    public class PostShortInfoDto
    {
        public string Id { get; set; }
        public string CoverImageUrl { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Author { get; set; }
        public bool IsLiked { get; set; } = false;

        public IEnumerable<string>? Tags { get; set; }
    }
}

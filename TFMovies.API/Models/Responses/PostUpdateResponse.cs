﻿namespace TFMovies.API.Models.Responses;

public class PostUpdateResponse
{
    public string Id { get; set; }
    public string CoverImageUrl { get; set; }
    public string Title { get; set; }
    public string HtmlContent { get; set; }    
    public DateTime CreatedAt { get; set; }    
    public string Author { get; set; }
    public string Theme { get; set; }
    
    public ICollection<string> Tags { get; set; }   
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("posts")]
[ApiController]
[Produces("application/json")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Create a new post.
    /// </summary>
    /// <param name="model">The post details.</param>
    /// <returns>Status 201 and Post details if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     POST /posts/create
    ///     {
    ///        "coverImageUrl": "http://example.com/image.jpg",
    ///        "theme": "Theme1",
    ///        "title": "Post Title",
    ///        "htmlContent": "<p>Post content here</p>",
    ///        "tags": ["tag1", "tag2", "tag3"]
    ///     }
    /// 
    /// </remarks>

    [HttpPost("create")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.Author)]
    [SwaggerResponse(201, "CREATED", typeof(PostCreateResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> CreatePostAsync([FromBody] CreatePostRequest model)
    {

        var result = await _postService.CreatePostAsync(model, User);

        return Ok(result);
    }
}

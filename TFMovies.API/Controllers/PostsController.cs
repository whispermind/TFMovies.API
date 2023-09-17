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
    /// <returns>Status 200 and Post details if successful.</returns>
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

    [HttpPost]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.Author)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(PostCreateResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> CreateAsync([FromBody] PostCreateRequest model)
    {

        var result = await _postService.CreateAsync(model, User);

        return Ok(result);
    }

    /// <summary>
    /// Updates the specified post.
    /// </summary>
    /// <param name="id">The identifier of the post to be updated.</param>
    /// <param name="model">An object containing the updated post details.</param>
    /// <returns>Returns status 200 along with the details of the updated post if the operation is successful.</returns>
    /// <remarks>
    /// Example of a PUT request to update a post:
    ///
    ///     PUT /posts/{id}
    ///     {
    ///        "coverImageUrl": "http://example.com/image.jpg",
    ///        "theme": "Theme2",
    ///        "title": "Updated Post Title",
    ///        "htmlContent": "<p>Updated post content here</p>",
    ///        "tags": ["tag4", "tag5"]
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.Author)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(PostUpdateResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] PostUpdateRequest model)
    {
        var result = await _postService.UpdateAsync(id, model);

        return Ok(result);
    }
}

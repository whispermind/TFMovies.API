using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using TFMovies.API.Common.Constants;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("comments")]
[ApiController]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly IPostService _postService;

    public CommentsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Searches for posts based on the provided query across post's Comments.
    /// </summary>
    /// <param name="model">
    /// A model containing pagination, sorting, and filtering parameters:
    /// - **Page**: The page number.
    /// - **Limit**: The maximum number of posts to retrieve.
    /// - **Sort**: The field by which to sort the posts. (Optional)
    /// - **Order**: The order in which to sort the posts (e.g., asc or desc). (Optional)
    /// </param>
    /// <param name="query">The search terms used to filter the posts.</param>
    /// <returns>Returns a status of 200 along with a list of posts that match the search criteria.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /comments/search?page=1&amp;limit=10&amp;query=term1,term2,...
    ///
    /// **Note**: You must be authenticated and have one of the following roles: Admin, Author, or User to access this endpoint.
    /// </remarks>
    [HttpGet("search")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<PostShortInfoDto>))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> SearchAsync([FromQuery] PagingSortFilterParams model, [FromQuery] string query)
    {

        var result = await _postService.SearchByCommentsWithPagingAsync(model, query, User);

        return Ok(result);
    }
}

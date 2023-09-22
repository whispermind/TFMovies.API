using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("tags")]
[ApiController]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly IPostService _postService;

    public TagsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Retrieves the list of tags by sort and limit optional.
    /// </summary>   
    /// <param name="limit">Limit the number of records returned.</param>
    /// <param name="sort">Specifies the criterion by which tags are sorted. It's an optional parameter with the default value set to "rated".</param>
    /// <param name="order">Specifies the order by which tags are sorted. It's an optional parameter with the default value set to "desc".</param>
    /// <returns>Returns status 200 along with the list of top tags if the operation is successful.</returns>
    /// <remarks>
    /// Example of a GET request to retrieve top tags:
    ///
    ///     GET /tags?limit=3
    ///     GET /tags?limit=3&amp;sort=rated&amp;order=desc
    ///  
    /// **Note**: You must be authenticated as an Admin, Author, or User to use this endpoint.
    /// </remarks>
    [HttpGet]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<TagDto>))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetTagsAsync(int limit, string sort = SortOptions.Rated, string order = "desc")
    {
        var result = await _postService.GetTagsAsync(limit, sort, order);

        return Ok(result);
    }
}

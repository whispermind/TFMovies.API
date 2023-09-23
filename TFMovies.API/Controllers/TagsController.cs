using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
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
    /// <param name="model">Contains optional parameters for limit, sort, and order of the returned records. The default values are: sort = "rated", order = "desc", and limit = 1.</param>    
    /// <returns>Returns status 200 along with the list of top tags if the operation is successful.</returns>
    /// <remarks>
    /// Example of a GET request to retrieve top tags:
    ///
    ///     GET /tags?limit=3
    ///     GET /tags?limit=3&amp;sort=rated&amp;order=desc
    ///      
    /// </remarks>
    [HttpGet]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<TagDto>))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetTagsAsync([FromQuery] SortFilterRequest model)
    {
        var result = await _postService.GetTagsAsync(model);

        return Ok(result);
    }
}

﻿using Microsoft.AspNetCore.Authorization;
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
    /// <param name="model">Contains optional parameters for limit, sort, and order of the returned records. The default values are: sort = "rated", order = "desc", and limit = 1.</param>    
    /// <returns>Returns status 200 along with the list of top tags if the operation is successful.</returns>
    /// <remarks>
    /// Example of a GET request to retrieve top tags:
    ///
    ///     GET /tags?limit=3
    ///      
    /// </remarks>
    [HttpGet]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<TagDto>))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetTagsAsync([FromQuery] PagingSortFilterParams model)
    {
        var result = await _postService.GetTagsAsync(model);

        return Ok(result);
    }

    /// <summary>
    /// Searches for posts based on the provided query across post's Tags.
    /// </summary>
    /// <param name="model">
    /// A model containing pagination, sorting, and filtering parameters:
    /// - **Page**: The page number. (Optional)
    /// - **Limit**: The maximum number of posts to retrieve. (Optional)
    /// - **Sort**: The field by which to sort the posts. (Optional)
    /// - **Order**: The order in which to sort the posts (e.g., asc or desc). (Optional)
    /// </param>
    /// <param name="query">The search terms used to filter the posts.</param>
    /// <returns>Returns a status of 200 along with a list of posts that match the search criteria.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /tags/search?page=1&amp;limit=10&amp;query=term1,term2,...
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

        var result = await _postService.SearchByTagsWithPagingAsync(model, query, User);

        return Ok(result);
    }
}

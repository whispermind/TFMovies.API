﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
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
    ///        "themeId": "Theme1_Id",
    ///        "title": "Post Title",
    ///        "htmlContent": "<p>Post content here</p>",
    ///        "tags": ["tag1", "tag2", "tag3"]
    ///     }
    /// 
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author)]
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
    ///        "themeId": "Theme2_Id",
    ///        "themeId": "Theme2_Id",
    ///        "title": "Updated Post Title",
    ///        "htmlContent": "<p>Updated post content here</p>",
    ///        "tags": ["tag4", "tag5"]
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(PostUpdateResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] PostUpdateRequest model)
    {
        var result = await _postService.UpdateAsync(id, model);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of posts based on the provided search and filter criteria.
    /// </summary>
    /// <param name="model">
    /// A model containing pagination, sorting, and filtering parameters:
    /// - **Page**: The page number. (Optional; default is 1)
    /// - **Limit**: The maximum number of posts to retrieve. (Optional; default is 10)
    /// - **Sort**: The field by which to sort the posts (e.g., "rated" or "created"). (Optional)
    /// - **Order**: The order in which to sort the posts (e.g., "asc" or "desc"). (Optional)
    /// - **ThemeId**: A specific theme ID to sort the posts by. (Optional)
    /// </param>
    /// <param name="queryModel">
    /// The search and filter criteria:
    /// - **Query**: Search terms used to filter the posts by title or content. (Optional)
    /// - **TagQuery**: Search terms to filter the posts by associated tags. (Optional)
    /// - **CommentQuery**: Search terms to filter the posts by associated comments. (Optional)
    /// </param>
    /// <returns>Returns a status of 200 along with a paginated list of posts that match the search and filter criteria.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /posts?page=1&amp;limit=10&amp;sort=created&amp;themeId=ThemeId1
    ///     GET /posts?page=1&amp;limit=10&amp;query=sample,sample1
    ///     GET /posts?page=1&amp;limit=10&amp;tagQuery=tag1,tag2
    ///     GET /posts?page=1&amp;limit=10&amp;commentQuery=commentText
    ///
    /// **Note**: This endpoint can be accessed by any user, even if they are not authenticated.
    /// </remarks>   
    [HttpGet]
    [AllowAnonymous]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(PostsPaginatedResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetAllAsync([FromQuery] PagingSortFilterParams model, [FromQuery] PostsQueryParams queryModel)
    {
        var result = await _postService.GetAllAsync(model, queryModel, User);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves the details of a specific post by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the post.</param>
    /// <param name="limit">Limit the number of records returned.</param>
    /// <returns>Returns status 200 along with the detailed information of the post if the operation is successful.</returns>
    /// <remarks>
    /// Example of a GET request to retrieve a post:
    ///
    ///     GET /posts/{id}?limit=3
    ///
    /// **Note**: You must be authenticated as an Admin, Author, or User to use this endpoint.
    /// </remarks>
    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(PostGetByIdResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetByIdAsync(string id, int limit)
    {
        var result = await _postService.GetByIdAsync(id, User, limit);

        return Ok(result);
    }

    /// <summary>
    /// Adds a comment to a specific post.
    /// </summary>
    /// <param name="id">Id of the specific post.</param>
    /// <param name="model">An object containing the details of the comment to be added.</param>
    /// <returns>Returns status 200 along with the details of the added comment if the operation is successful.</returns>
    /// <remarks>
    /// Example of a POST request to add a comment:
    ///
    ///     POST /posts/{id}/comments
    ///     {        
    ///        "content": "This is a comment"
    ///     }
    ///
    /// **Note**: You must be authenticated as an Admin, Author, or User to use this endpoint.
    /// </remarks>
    [HttpPost("{id}/comments")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(PostAddCommentResponse))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> AddCommentAsync([FromRoute] string id, [FromBody] PostAddCommentRequest model)
    {
        var result = await _postService.AddCommentAsync(id, model, User);

        return Ok(result);
    }

    /// <summary>
    /// Adds a like to a specific post.
    /// </summary>
    /// <param name="id">The identifier of the post to be liked.</param>
    /// <returns>Returns status 200 if the operation is successful.</returns>
    /// <remarks>
    /// Example of a POST request to add a like to a post:
    ///
    ///     POST /posts/{id}/likes
    ///
    /// **Note**: You must be authenticated as an Admin, Author, or User to use this endpoint.
    /// </remarks>
    [HttpPost("{id}/likes")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> AddLikeAsync([FromRoute] string id)
    {
        await _postService.AddLikeAsync(id, User);

        return Ok();
    }

    /// <summary>
    /// Removes a like from a specific post.
    /// </summary>
    /// <param name="id">The identifier of the post from which the like will be removed.</param>
    /// <returns>Returns status 200 if the operation is successful.</returns>
    /// <remarks>
    /// Example of a DELETE request to remove a like from a post:
    ///
    ///     DELETE /posts/{id}/likes
    ///
    /// **Note**: You must be authenticated as an Admin, Author, or User to use this endpoint.
    /// </remarks>
    [HttpDelete("{id}/likes")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(204, "NO_CONTENT")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> RemoveLikeAsync([FromRoute] string id)
    {
        await _postService.RemoveLikeAsync(id, User);

        return NoContent();
    }

    /// <summary>
    /// Retrieves the posts liked by the authenticated user.
    /// </summary>
    /// <param name="model">A model containing pagination, sorting, and filtering parameters:
    /// - **Page**: The page number. (Optional)
    /// - **Limit**: The maximum number of posts to retrieve. (Optional)
    /// - **Sort**: The field by which to sort the posts. (Optional)
    /// - **Order**: The order in which to sort the posts (e.g., ascending or descending). (Optional)    
    /// </param>
    /// <returns>Returns a status of 200 along with a list of posts' brief details if the operation is successful.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /posts/liked-by/me?page=1&amp;limit=10
    ///
    /// **Note**: You must be authenticated and have one of the following roles: Admin, Author, or User to access this endpoint.
    /// </remarks>
    [HttpGet("liked-by/me")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<PostShortInfoDto>))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetUserFavoritePostAsync([FromQuery] PagingSortFilterParams model)
    {
        var result = await _postService.GetUserFavoritePostAsync(model, User);

        return Ok(result);
    }   
}

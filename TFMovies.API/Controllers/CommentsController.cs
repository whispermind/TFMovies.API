using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("comments")]
[ApiController]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Deletes a comment by its Id.
    /// </summary>
    /// <param name="id">The Id of the comment to delete.</param>
    /// <returns>Status 204 if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     DELETE /comments/{id}
    /// 
    /// Note: 
    /// - Accessible only to users who are authenticated and hold the 'Admin', 'Author', or 'User' roles.
    /// - The comment can be deleted by the author of the comment or the author of the linked post.
    /// </remarks>
    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Author + "," + RoleNames.User)]
    [SwaggerResponse(204, "NO_CONTENT")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(403, "FORBIDDEN", typeof(ErrorResponse))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _commentService.DeleteAsync(id, User);

        return NoContent();
    }
}

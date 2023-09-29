using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("roles")]
[ApiController]
[Produces("application/json")]

public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Get all user roles.
    /// </summary>
    /// <returns>A list of roles.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     GET /roles
    /// 
    /// </remarks>
    [HttpGet]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<RoleDto>))]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _roleService.GetAllAsync();

        return Ok(result);
    }
}

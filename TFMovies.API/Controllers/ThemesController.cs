﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TFMovies.API.Common.Constants;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;
using TFMovies.API.Services.Interfaces;

namespace TFMovies.API.Controllers;

[Route("themes")]
[ApiController]
[Produces("application/json")]
[Authorize(Roles = RoleNames.Admin)]
public class ThemesController : ControllerBase
{
    private readonly IThemeService _themeService;

    public ThemesController(IThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Create a new theme.
    /// </summary>
    /// <param name="model">The theme creation details.</param>
    /// <returns>Status 201 if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     POST /themes
    ///     {
    ///        "name": "NewThemeName"
    ///     }
    /// 
    /// Note: Accessible only to users who are authenticated and hold the 'Admin' role.
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(Tags = new[] { "Helpers" })]
    [SwaggerResponse(201, "CREATED")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ThemeCreateRequest model)
    {
        await _themeService.CreateAsync(model.Name);

        return StatusCode(StatusCodes.Status201Created);
    }

    /// <summary>
    /// Get all themes.
    /// </summary>
    /// <returns>A list of themes.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     GET /themes
    /// 
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Tags = new[] { "Helpers" })]
    [SwaggerResponse(200, "REQUEST_SUCCESSFULL", typeof(IEnumerable<ThemeDto>))]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> GetAllAsync()
    {
        var result = await _themeService.GetAllAsync();

        return Ok(result);
    }

    /// <summary>
    /// Update an existing theme.
    /// </summary>
    /// <param name="model">The theme update details.</param>
    /// <returns>Status 204 if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     PUT /themes
    ///     {
    ///        "oldName": "ExistingThemeName",
    ///        "newName": "NewThemeName"
    ///     }
    /// 
    /// Note: Accessible only to users who are authenticated and hold the 'Admin' role.
    /// </remarks>
    [HttpPut]
    [SwaggerOperation(Tags = new[] { "Helpers" })]
    [SwaggerResponse(204, "NO_CONTENT")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> UpdateAsync(ThemeUpdateRequest model)
    {
        await _themeService.UpdateAsync(model);

        return NoContent();
    }

    /// <summary>
    /// Delete a theme by name.
    /// </summary>
    /// <param name="id">Id of the theme to delete.</param>
    /// <returns>Status 204 if successful.</returns>
    /// <remarks>
    /// Example:
    /// 
    ///     DELETE /themes/ThemeName
    /// 
    /// Note: Accessible only to users who are authenticated and hold the 'Admin' role.
    /// </remarks>
    [HttpDelete("{id}")]
    [SwaggerOperation(Tags = new[] { "Helpers" })]
    [SwaggerResponse(204, "NO_CONTENT")]
    [SwaggerResponse(400, "BAD_REQUEST", typeof(ErrorResponse))]
    [SwaggerResponse(401, "UNAUTHORIZED")]
    [SwaggerResponse(500, "INTERNAL_SERVER_ERROR", typeof(ErrorResponse))]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _themeService.DeleteAsync(id);

        return NoContent();
    }
}

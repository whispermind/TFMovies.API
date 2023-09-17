using System.Security.Claims;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IPostService
{
    public Task<PostCreateResponse> CreateAsync(PostCreateRequest model, ClaimsPrincipal currentUserPrincipal);

    public Task<PostUpdateResponse> UpdateAsync(string id, PostUpdateRequest model);
}

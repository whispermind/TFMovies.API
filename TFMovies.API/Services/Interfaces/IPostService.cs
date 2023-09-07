using System.Security.Claims;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IPostService
{
    public Task<PostCreateResponse> CreatePostAsync(CreatePostRequest model, ClaimsPrincipal currentUserPrincipal);
}

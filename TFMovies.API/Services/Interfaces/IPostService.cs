using System.Security.Claims;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IPostService
{
    public Task<PostCreateResponse> CreateAsync(PostCreateRequest model, ClaimsPrincipal currentUserPrincipal);
    public Task<PostUpdateResponse> UpdateAsync(string id, PostUpdateRequest model);
    public Task<PostGetAllResponse> GetAllAsync(int page, int limit, string? sort, string? theme, ClaimsPrincipal currentUserPrincipal);
    public Task<PostGetByIdResponse> GetByIdAsync(string id, ClaimsPrincipal currentUserPrincipal);
    public Task<PostAddCommentResponse> AddCommentAsync(PostAddCommentRequest model, ClaimsPrincipal currentUserPrincipal);
    public Task AddLikeAsync(string postId, ClaimsPrincipal currentUserPrincipal);
    public Task RemoveLikeAsync(string postId, ClaimsPrincipal currentUserPrincipal);
}

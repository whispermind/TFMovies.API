using System.Security.Claims;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IPostService
{
    public Task<PostCreateResponse> CreateAsync(PostCreateRequest model, ClaimsPrincipal currentUserPrincipal);
    public Task<PostUpdateResponse> UpdateAsync(string id, PostUpdateRequest model);
    public Task<PostsPaginatedResponse> GetAllAsync(PagingSortFilterParams model, ClaimsPrincipal currentUserPrincipal);
    public Task<PostGetByIdResponse> GetByIdAsync(string id, ClaimsPrincipal currentUserPrincipal, int limit);
    public Task<PostAddCommentResponse> AddCommentAsync(string id, PostAddCommentRequest model, ClaimsPrincipal currentUserPrincipal);
    public Task AddLikeAsync(string id, ClaimsPrincipal currentUserPrincipal);
    public Task RemoveLikeAsync(string id, ClaimsPrincipal currentUserPrincipal);
    public Task<IEnumerable<TagDto>> GetTagsAsync(PagingSortFilterParams model);
    public Task<PostsPaginatedResponse> GetUserFavoritePostAsync(PagingSortFilterParams model, ClaimsPrincipal currentUserPrincipal);
    public Task<PostsPaginatedResponse> SearchWithPagingAsync(PagingSortFilterParams model, string query, ClaimsPrincipal currentUserPrincipal);
    public Task<PostsPaginatedResponse> SearchByTagsWithPagingAsync(PagingSortFilterParams model, string query, ClaimsPrincipal currentUserPrincipal);
    public Task<PostsPaginatedResponse> SearchByCommentsWithPagingAsync(PagingSortFilterParams model, string query, ClaimsPrincipal currentUserPrincipal);
}

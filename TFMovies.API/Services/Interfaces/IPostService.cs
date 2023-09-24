using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IPostService
{
    public Task<PostCreateResponse> CreateAsync(PostCreateRequest model, ClaimsPrincipal currentUserPrincipal);
    public Task<PostUpdateResponse> UpdateAsync(string id, PostUpdateRequest model);
    public Task<PostsPaginatedResponse> GetAllAsync(PaginationSortFilterParams model, ClaimsPrincipal currentUserPrincipal);
    public Task<PostGetByIdResponse> GetByIdAsync(string id, ClaimsPrincipal currentUserPrincipal, int limit);
    public Task<PostAddCommentResponse> AddCommentAsync(string id, PostAddCommentRequest model, ClaimsPrincipal currentUserPrincipal);
    public Task AddLikeAsync(string id, ClaimsPrincipal currentUserPrincipal);
    public Task RemoveLikeAsync(string id, ClaimsPrincipal currentUserPrincipal);
    public Task<IEnumerable<TagDto>> GetTagsAsync(PaginationSortFilterParams model);    
    public Task<PostsPaginatedResponse> GetUserFavoritePostAsync(PaginationSortFilterParams model, ClaimsPrincipal currentUserPrincipal);
}

using System.Security.Claims;

namespace TFMovies.API.Services.Interfaces;

public interface ICommentService
{
    public Task DeleteAsync(string id, ClaimsPrincipal currentUserPrincipal);
}

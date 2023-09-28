using System.Security.Claims;
using TFMovies.API.Data.Entities;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Utils;

public static class UserUtils
{ 
    public static async Task<User?> GetUserByIdFromClaimAsync(IUserRepository userRepository, ClaimsPrincipal currentUserPrincipal)
    {
        var userId = currentUserPrincipal.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await userRepository.FindByIdAsync(userId);
    }

    public static void CheckCurrentUserFoundOrThrow(User? user)
    {
        //if (user == null)
        //{
        //    throw new ServiceException(HttpStatusCode.Unauthorized);
        //}
    }
}

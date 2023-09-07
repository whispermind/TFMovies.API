using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IUserRepository
{
    //CRUD User
    public Task<IdentityResult> CreateAsync(User user, string password);
    public Task<User> FindByIdAsync(string userId);
    public Task<User> FindByEmailAsync(string email);
    public Task<IdentityResult> UpdateAsync(User user);
    public Task<IdentityResult> DeleteAsync(User user);
    public Task<User?> GetCurrentAuthenticatedUser(ClaimsPrincipal currentUserPrincipal);

    //Manage Password
    public Task<bool> CheckPasswordAsync(User user, string password);
    public string HashPassword(User user, string password);

    //Manage UserRoles
    public Task<IdentityResult> AddToRoleAsync(User user, string role);
    public Task<IEnumerable<string>> GetRolesAsync(User user);
    public Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles);
}

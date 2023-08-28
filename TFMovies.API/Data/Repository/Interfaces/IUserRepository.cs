using Microsoft.AspNetCore.Identity;
using TFMovies.API.Data.Entities;

namespace TFMovies.API.Data.Repository.Interfaces;

public interface IUserRepository
{
    //CRUD User
    public Task<IdentityResult> CreateAsync(User user, string password);
    public ValueTask<User> FindByIdAsync(string userId);
    public ValueTask<User> FindByEmailAsync(string email);
    public Task<IdentityResult> UpdateAsync(User user);
    public Task<IdentityResult> DeleteAsync(User user);

    //Manage Password
    public Task<bool> CheckPasswordAsync(User user, string password);
    public string HashPassword(User user, string password);

    //Manage UserRoles
    public Task<IdentityResult> AddToRoleAsync(User user, string role);
    public Task<IEnumerable<string>> GetRolesAsync(User user);
}

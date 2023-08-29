using Microsoft.AspNetCore.Identity;
using TFMovies.API.Data.Entities;
using TFMovies.API.Data.Repository.Interfaces;

namespace TFMovies.API.Data.Repository.Implementations;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    //CRUD User
    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }
    public async ValueTask<User> FindByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }
    public async Task<User> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }
    public async Task<IdentityResult> UpdateAsync(User user)
    {
        return await _userManager.UpdateAsync(user);
    }
    public async Task<IdentityResult> DeleteAsync(User user)
    {
        return await _userManager.DeleteAsync(user);
    }

    //Manage Password
    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }    
    public string HashPassword(User user, string password) 
    {
        var hasher = new PasswordHasher<User>();
        return hasher.HashPassword(user, password);
    }


    //Manage UserRoles
    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        return await _userManager.AddToRoleAsync(user, role);
    }
    public async Task<IEnumerable<string>> GetRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;
using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
using TFMovies.API.Filters;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

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
    
    public async Task<User> FindByIdAsync(string userId)
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

    public async Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> userIds)
    {
        var users = new List<User>();

        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user != null)
            {
                users.Add(user);
            }
        }
        return users;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = await _userManager.Users.ToListAsync();

        return users;
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

    public async Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
    {
        return await _userManager.RemoveFromRolesAsync(user, roles);
    }

    public async Task<bool> IsInRoleAsync(User user, string role)
    {
        return await _userManager.IsInRoleAsync(user, role);
    }
}

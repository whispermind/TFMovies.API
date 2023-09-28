using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TFMovies.API.Common.Constants;
using TFMovies.API.Data;
using TFMovies.API.Data.Entities;
using TFMovies.API.Extensions;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context;
    public IEnumerable<string> SearchColumns => new[] { "Nickname", "Email" };

    public UserRepository(UserManager<User> userManager, DataContext context)
    {
        _userManager = userManager;
        _context = context;
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

    public async Task<PagedResult<UserRoleDto>> GetAllPagingAsync(PagingSortParams pagingSortModel, UsersFilterParams filterModel, UsersQueryDto queryDto)
    {
        var query = _userManager.Users;

        // Filter by Role
        if (!string.IsNullOrEmpty(filterModel.RoleId))
        {
            query = query
                .Where(u => _context.UserRoles
                    .Any(ur => ur.UserId == u.Id && ur.RoleId == filterModel.RoleId));
        }
        // Search
        if (queryDto.Query != null && queryDto.Query.Any())
        {
            query = query.SearchByTerms(SearchColumns, queryDto.Query);
        }

        var userRoleQuery = from user in query
                            join userRole in _context.UserRoles on user.Id equals userRole.UserId
                            join role in _context.Roles on userRole.RoleId equals role.Id
                            select new UserRoleDto
                            {
                                User = user,
                                Role = new RoleDto
                                {
                                    Name = role.Name,
                                    Id = role.Id
                                }
                            };

        Expression<Func<UserRoleDto, object>> sortSelector = pagingSortModel.Sort switch
        {
            SortOptions.Email => dto => dto.User.Email,
            SortOptions.RoleName => dto => dto.Role.Name,            
            _ => dto => dto.User.Nickname // default
        };

        var pagingSortDto = pagingSortModel.ToPagingDto(sortSelector);

        var pagedUserRoles = await userRoleQuery.GetPagedDataAsync<UserRoleDto>(pagingSortDto);

        return pagedUserRoles;
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

    //helpers
    private Expression<Func<User, object>> GetSortSelector(string? sort)
    {
        return sort switch
        {
            SortOptions.Email => u => u.Email,
            _ => u => u.Nickname // default
        };
    }
}

﻿using Microsoft.AspNetCore.Identity;
using TFMovies.API.Data.Entities;
using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Repositories.Interfaces;

public interface IUserRepository
{
    //CRUD User
    public Task<IdentityResult> CreateAsync(User user, string password);
    public IQueryable<User> QueryActiveUsersOnly();
    public IQueryable<User> QueryAllUsers();
    public Task<User?> FindByIdAsync(string userId);
    public Task<User?> FindByEmailAsync(string email);
    public Task<IdentityResult> UpdateAsync(User user);
    public Task<IdentityResult> DeleteAsync(User user);
    public Task<IEnumerable<User>> GetUsersByIdsAsync(IEnumerable<string> userIds);
    public Task<IEnumerable<User>> GetAllAsync();
    public Task<PagedResult<UserRoleDto>> GetAllPagingAsync(PagingSortParams pagingSortModel, UsersFilterParams filterModel, UsersQueryDto queryDto);
    public Task<IdentityResult> SoftDeleteAsync(User user);   

    //Manage Password
    public Task<bool> CheckPasswordAsync(User user, string password);
    public string HashPassword(User user, string password);

    //Manage UserRoles
    public Task<IdentityResult> AddToRoleAsync(User user, string role);
    public Task<IEnumerable<string>> GetRolesAsync(User user);
    public Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles);
    public Task<bool> IsInRoleAsync(User user, string role);
    public Task<RoleDto?> GetUserRoleDetailsAsync(User user);
}

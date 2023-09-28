using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TFMovies.API.Models.Dto;
using TFMovies.API.Repositories.Interfaces;

namespace TFMovies.API.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleRepository(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        return await _roleManager.Roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name
        }).ToListAsync();
    }
}

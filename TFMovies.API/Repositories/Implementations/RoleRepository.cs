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
        var result = await _roleManager.Roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name
        }).ToListAsync();

        return result;
    }

    public async Task<IdentityRole> FindByIdAsync(string id)
    {
        var result = await _roleManager.FindByIdAsync(id);
        
        return result;
    }
}

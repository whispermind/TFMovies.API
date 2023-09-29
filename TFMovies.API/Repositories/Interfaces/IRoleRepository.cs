using Microsoft.AspNetCore.Identity;
using TFMovies.API.Models.Dto;

namespace TFMovies.API.Repositories.Interfaces;

public interface IRoleRepository
{
    public Task<IEnumerable<RoleDto>> GetAllAsync();
    public Task<IdentityRole> FindByIdAsync(string id);
}

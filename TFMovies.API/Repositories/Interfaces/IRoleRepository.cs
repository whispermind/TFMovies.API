using TFMovies.API.Models.Dto;

namespace TFMovies.API.Repositories.Interfaces;

public interface IRoleRepository
{
    public Task<IEnumerable<RoleDto>> GetAllAsync();
}

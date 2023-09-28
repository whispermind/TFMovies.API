using TFMovies.API.Models.Dto;
using TFMovies.API.Models.Responses;

namespace TFMovies.API.Services.Interfaces;

public interface IRoleService
{
    public Task<IEnumerable<RoleDto>> GetAllAsync();
}

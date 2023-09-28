using TFMovies.API.Data.Entities;

namespace TFMovies.API.Models.Dto;

public class UserRoleDto
{
    public User User { get; set; }
    public RoleDto Role { get; set; }   
}

namespace TFMovies.API.Models.Dto;

public class UserShortInfoDto
{    
    public string Id { get; set; }
    public string Nickname { get; set; }
    public string Email { get; set; }
    public RoleDto Role { get; set; }
}

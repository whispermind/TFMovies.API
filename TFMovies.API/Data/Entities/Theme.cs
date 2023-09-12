using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public class Theme : BaseModel
{    
    [MaxLength(255)]
    public string Name { get; set; }

    public ICollection<Post>? Posts { get; set; }
}

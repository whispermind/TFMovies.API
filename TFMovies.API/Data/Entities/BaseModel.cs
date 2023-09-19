using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public abstract class BaseModel
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

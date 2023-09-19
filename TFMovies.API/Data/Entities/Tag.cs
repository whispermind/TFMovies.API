using Microsoft.EntityFrameworkCore;

namespace TFMovies.API.Data.Entities;

[Index(nameof(Name), IsUnique = true)]
public class Tag : BaseModel
{
    public string Name { get; set; }

    public ICollection<PostTag>? PostTags { get; set; }
}

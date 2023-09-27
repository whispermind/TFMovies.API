using Microsoft.AspNetCore.Identity;

namespace TFMovies.API.Data.Entities;

public class User : IdentityUser
{
    public string Nickname { get; set; }
    public ICollection<UserActionToken>? UserActionTokens { get; set; }
    public ICollection<RefreshToken>? RefreshTokens { get; set; }
    public ICollection<PostComment>? CommentedPosts { get; set; }
    public ICollection<PostLike>? LikedPosts { get; set; }
}
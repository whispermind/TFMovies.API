using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public class User : IdentityUser
{
    public string Nickname { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsRequestForNewRole { get; set; } = false;
    public ICollection<UserActionToken>? UserActionTokens { get; set; }
    public ICollection<RefreshToken>? RefreshTokens { get; set; }
    public ICollection<PostComment>? CommentedPosts { get; set; }
    public ICollection<PostLike>? LikedPosts { get; set; }
}
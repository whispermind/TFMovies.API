using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using TFMovies.API.Common.Enum;

namespace TFMovies.API.Data.Entities;

public class UserActionToken
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; }

    [Required]
    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public ActionTokenTypeEnum TokenType { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [DefaultValue(false)]
    public bool IsUsed { get; set; } = false;

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsActive => !IsUsed && !IsExpired;
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TFMovies.API.Data.Entities;

public class RefreshToken
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public string CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsActive => RevokedAt == null && !IsExpired;
}

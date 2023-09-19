using System.ComponentModel.DataAnnotations.Schema;

namespace TFMovies.API.Data.Entities;

public class RefreshToken : BaseModel
{   
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }
    
    public string Token { get; set; }
    
    public DateTime ExpiresAt { get; set; }
   
    public DateTime CreatedAt { get; set; }
    
    public string CreatedByIp { get; set; }

    public DateTime? RevokedAt { get; set; }

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsActive => RevokedAt == null && !IsExpired;
}

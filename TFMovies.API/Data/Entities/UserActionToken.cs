using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using TFMovies.API.Common.Enum;
using Microsoft.EntityFrameworkCore;

namespace TFMovies.API.Data.Entities;

[Index(nameof(UserId), nameof(TokenType), IsUnique = true)]
public class UserActionToken : BaseModel
{   
    public string UserId { get; set; }
        
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    public ActionTokenTypeEnum TokenType { get; set; }
    
    public string Token { get; set; }
        
    public DateTime ExpiresAt { get; set; }
    
    public DateTime CreatedAt { get; set; }

    [DefaultValue(false)]
    public bool IsUsed { get; set; } = false;

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsActive => !IsUsed && !IsExpired;
}

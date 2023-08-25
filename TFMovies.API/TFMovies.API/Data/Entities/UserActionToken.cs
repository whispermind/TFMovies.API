using TFMovies.API.Common.Enum;

namespace TFMovies.API.Data.Entities;

public class UserActionToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public ActionTokenTypeEnum TokenType { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public bool IsUsed { get; set; } = false;   
    public User User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => !IsUsed && !IsExpired;
}

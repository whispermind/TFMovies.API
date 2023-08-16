using TFMovies.API.Common.Enum;

namespace TFMovies.API.Data.Entities;

public class UserSecretToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public SecretTokenTypeEnum TokenType { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
}

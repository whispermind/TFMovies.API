namespace TFMovies.API.Data.Entities;

public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public User User { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string CreatedByIp { get; set; }
    public DateTime? Revoked { get; set; }    
    public bool IsExpired => DateTime.UtcNow >= Expires;    
    public bool IsActive => Revoked == null && !IsExpired;    
}

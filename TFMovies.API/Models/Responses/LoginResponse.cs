namespace TFMovies.API.Models.Responses
{
    public class LoginResponse
    {
        public CurrentUser CurrentUser { get; set; }        
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class CurrentUser
    {
        public string Id { get; set; }
        public string Nickname { get; set; }
        public string Role { get; set; }
    }
}
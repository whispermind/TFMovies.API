namespace TFMovies.API.Integrations;

public interface IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}

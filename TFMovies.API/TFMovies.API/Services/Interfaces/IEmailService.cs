namespace TFMovies.API.Services.Interfaces;

public interface IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
}

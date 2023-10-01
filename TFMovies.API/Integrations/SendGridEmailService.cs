using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Text.RegularExpressions;
using TFMovies.API.Common.Constants;
using TFMovies.API.Exceptions;
using TFMovies.API.Models.Dto;
using System.Net;
using TFMovies.API.Services.Implementations;
using TFMovies.API.Models.Requests;

namespace TFMovies.API.Integrations;

public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _client;
    private readonly ILogger _logger;
    private readonly SendGridSettings _sendGridSettings;    

    public SendGridEmailService(
        ISendGridClient client,
        IOptions<SendGridSettings> sendGridSettings,
        ILogger<SendGridEmailService> logger)
        
    {
        _client = client;
        _logger = logger;
        _sendGridSettings = sendGridSettings.Value;
       
    }
    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        var from = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.FromName);
        var to = new EmailAddress(toEmail);

        var plainTextContent = Regex.Replace(htmlMessage, "<[^>]*>", "");
        var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlMessage);
        var response = await _client.SendEmailAsync(message);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(string.Format("Email with subject \"{0}\" to {1} has been sent successfully", subject, toEmail));
        }
        else
        {
            throw new ServiceException(HttpStatusCode.InternalServerError, ErrorMessages.OperationFailed);
        }
    }
}

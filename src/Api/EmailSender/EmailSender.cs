using Api.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Api.EmailSender;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly ApplicationConfiguration _configuration;

    public EmailSender(ILogger<EmailSender> logger, ApplicationConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmail(string toEmail, string subject, string message, CancellationToken ct)
    {
        var sendGridClient = new SendGridClient(_configuration.SendGrid.ApiKey);
        var sendGridMessage = new SendGridMessage
        {
            From = new EmailAddress("no-reply@aurokk.com", "Account management"),
            Subject = subject,
            // PlainTextContent = message,
            HtmlContent = message,
        };
        sendGridMessage.AddTo(toEmail);
        sendGridMessage.SetClickTracking(false, false);
        var sendGridResponse = await sendGridClient.SendEmailAsync(sendGridMessage, ct);
        if (sendGridResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation("Confirmation email sent to {ToEmail}", toEmail);
        }
    }
}
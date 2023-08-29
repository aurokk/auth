namespace Api.EmailSender;

public interface IEmailSender
{
    Task SendEmail(string toEmail, string subject, string message, CancellationToken ct);
}
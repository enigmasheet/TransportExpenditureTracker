using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TransportExpenditureTracker.Services;

public class EmailSender : IEmailSender
{
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailSender(IConfiguration configuration)
    {
        _apiKey = configuration["SendGrid:ApiKey"] ?? string.Empty;
        _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@expensetracker.com";
        _fromName = configuration["SendGrid:FromName"] ?? "Expense Tracker";
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(_apiKey))
            return;

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = subject,
            HtmlContent = htmlMessage
        };
        msg.AddTo(new EmailAddress(email));

        await client.SendEmailAsync(msg);
    }
}

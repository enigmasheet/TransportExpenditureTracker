using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace TransportExpenditureTracker.Services;

public class EmailSender : IEmailSender
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _smtpHost = configuration["Resend:SmtpHost"] ?? "smtp.resend.com";
        _smtpPort = int.TryParse(configuration["Resend:SmtpPort"], out var port) ? port : 587;
        _smtpUser = configuration["Resend:SmtpUser"] ?? "resend";
        _smtpPassword = configuration["Resend:ApiKey"] ?? string.Empty;
        _fromEmail = configuration["Resend:FromEmail"] ?? "noreply@expensetracker.com";
        _fromName = configuration["Resend:FromName"] ?? "Expense Tracker";
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(_smtpPassword))
            return;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress("", email));
        message.Subject = subject;

        var body = new TextPart("html") { Text = htmlMessage };
        message.Body = body;

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpUser, _smtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendEmailWithAttachmentAsync(string toEmail, string ccEmail, string subject, string body, byte[] attachmentBytes, string attachmentFileName)
    {
        if (string.IsNullOrEmpty(_smtpPassword))
            return;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Cc.Add(new MailboxAddress("", ccEmail));
        message.Subject = subject;

        var multipart = new Multipart("mixed");
        multipart.Add(new TextPart("plain") { Text = body });

        var attachment = new MimePart(GetMimeType(attachmentFileName))
        {
            Content = new MimeContent(new MemoryStream(attachmentBytes)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = attachmentFileName
        };
        multipart.Add(attachment);
        message.Body = multipart;

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpUser, _smtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
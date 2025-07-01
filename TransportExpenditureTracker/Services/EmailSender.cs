using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.ComponentModel.DataAnnotations;
using TransportExpenditureTracker.Models;

namespace TransportExpenditureTracker.Services
{
    public class EmailSender : IEmailSender<ApplicationUser>
    {
        private readonly ILogger _logger;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor, ILogger<EmailSender> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public AuthMessageSenderOptions Options { get; }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(Options.SendGridKey))
            {
                throw new Exception("Null SendGridKey");
            }

            await Execute(Options.SendGridKey, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(Options.SenderEmail, Options.SenderName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            msg.AddTo(new EmailAddress(toEmail));
            msg.SetClickTracking(false, false);

            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation(response.IsSuccessStatusCode
                ? $"Email to {toEmail} queued successfully!"
                : $"Failure Email to {toEmail}");
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Invalid email address", nameof(email));
            }

            string subject = "Confirm your email";
            string message = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";
            await SendEmailAsync(email, subject, message);
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Invalid email address", nameof(email));
            }

            string subject = "Reset your password";
            string message = $"Reset your password by <a href='{resetLink}'>clicking here</a>.";
            await SendEmailAsync(email, subject, message);
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Invalid email address", nameof(email));
            }

            string subject = "Password Reset Code";
            string message = $"Your password reset code is: <strong>{resetCode}</strong>";
            await SendEmailAsync(email, subject, message);
        }
    }
}

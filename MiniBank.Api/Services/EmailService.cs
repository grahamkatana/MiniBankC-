using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MiniBank.Api.Interfaces;

namespace MiniBank.Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendTransactionNotificationAsync(
            string toEmail,
            string transactionType,
            decimal amount,
            string accountNumber
        )
        {
            var subject = $"Transaction Alert: {transactionType}";
            var body =
                $@"
                <h2>Transaction Notification</h2>
                <p><strong>Type:</strong> {transactionType}</p>
                <p><strong>Amount:</strong> R {amount:N2}</p>
                <p><strong>Account:</strong> {accountNumber}</p>
                <p><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to MiniBank!";
            var body =
                $@"
                <h2>Welcome {userName}!</h2>
                <p>Your account has been created successfully.</p>
                <p>You can now start using MiniBank services.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(
                new MailboxAddress(
                    _configuration["Email:FromName"],
                    _configuration["Email:FromAddress"]
                )
            );
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "1025");

                // Connect (MailHog doesn't use TLS on port 1025)
                if (smtpPort == 1025)
                {
                    await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
                }
                else if (smtpPort == 465)
                {
                    await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect);
                }
                else
                {
                    await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                }

                // Authenticate only if credentials provided
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await smtp.AuthenticateAsync(username, password);
                }

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                Console.WriteLine($"✅ Email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email failed to {toEmail}: {ex.Message}");
            }
        }
    }
}

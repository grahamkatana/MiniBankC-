using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MiniBank.Api.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MiniBank.Api.Services
{
 


    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendTransactionNotificationAsync(string toEmail, string transactionType, decimal amount, string accountNumber)
        {
            var subject = $"Transaction Alert: {transactionType}";
            var body = $@"
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
            var body = $@"
                <h2>Welcome {userName}!</h2>
                <p>Your account has been created successfully.</p>
                <p>You can now start using MiniBank services.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _configuration["Email:FromName"],
                _configuration["Email:FromAddress"]
            ));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(
                    _configuration["Email:SmtpHost"],
                    int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the transaction
                Console.WriteLine($"Email failed: {ex.Message}");
            }
        }
    }
}
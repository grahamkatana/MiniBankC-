
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace MiniBank.Api.Interfaces;

public interface IEmailService
{
    Task SendTransactionNotificationAsync(string toEmail, string transactionType, decimal amount, string accountNumber);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
}

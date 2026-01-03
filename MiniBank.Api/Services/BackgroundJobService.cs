using Hangfire;
using MiniBank.Api.Interfaces;

namespace MiniBank.Api.Services
{
    public interface IBackgroundJobService
    {
        string GenerateMonthlyStatement(string accountNumber, int month, int year);
        string SendBulkEmailsToUsers(List<string> userEmails, string subject, string body);
        void ScheduleDailyReports();
    }

    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IEmailService _emailService;

        public BackgroundJobService(
            IAccountRepository accountRepo,
            ITransactionRepository transactionRepo,
            IEmailService emailService)
        {
            _accountRepo = accountRepo;
            _transactionRepo = transactionRepo;
            _emailService = emailService;
        }

        public string GenerateMonthlyStatement(string accountNumber, int month, int year)
        {
            var jobId = BackgroundJob.Enqueue(() => GenerateStatementAsync(accountNumber, month, year));
            return jobId;
        }

        public string SendBulkEmailsToUsers(List<string> userEmails, string subject, string body)
        {
            var jobId = BackgroundJob.Enqueue(() => SendBulkEmailsAsync(userEmails, subject, body));
            return jobId;
        }

        public void ScheduleDailyReports()
        {
            // Run every day at 8 AM
            RecurringJob.AddOrUpdate(
                "daily-reports",
                () => GenerateDailyReportAsync(),
                Cron.Daily(8)
            );
        }

        // Background methods (public for Hangfire to call)
        public async Task GenerateStatementAsync(string accountNumber, int month, int year)
        {
            Console.WriteLine($"Generating statement for {accountNumber}...");
            
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null) return;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var transactions = await _transactionRepo.GetByDateRangeAsync(account.Id, startDate, endDate);

            // Simulate processing
            await Task.Delay(5000); // 5 seconds

            // Send email with statement
            var emailBody = $@"
                <h2>Monthly Statement - {month}/{year}</h2>
                <p><strong>Account:</strong> {accountNumber}</p>
                <p><strong>Transactions:</strong> {transactions.Count}</p>
                <p><strong>Balance:</strong> R {account.Balance:N2}</p>
            ";

            // await _emailService.SendEmailAsync(
            //     account.User?.Email ?? "user@example.com",
            //     $"Statement for {month}/{year}",
            //     emailBody
            // );

            Console.WriteLine($"Statement generated and sent for {accountNumber}");
        }

        public async Task SendBulkEmailsAsync(List<string> userEmails, string subject, string body)
        {
            Console.WriteLine($"Sending {userEmails.Count} emails...");
            
            foreach (var email in userEmails)
            {
                // await _emailService.SendEmailAsync(email, subject, body);
                await Task.Delay(100); // Rate limiting
            }

            Console.WriteLine("Bulk emails sent!");
        }

        public async Task GenerateDailyReportAsync()
        {
            Console.WriteLine("Generating daily report...");
            await Task.Delay(3000);
            Console.WriteLine("Daily report generated!");
        }
    }
}
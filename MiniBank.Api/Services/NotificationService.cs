using Microsoft.AspNetCore.SignalR;
using MiniBank.Api.Hubs;

namespace MiniBank.Api.Services
{
    public interface INotificationService
    {
        Task NotifyTransactionAsync(string accountNumber, object transactionData);
        Task NotifyBalanceChangeAsync(string accountNumber, decimal newBalance);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<TransactionHub> _hubContext;

        public NotificationService(IHubContext<TransactionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyTransactionAsync(string accountNumber, object transactionData)
        {
            await _hubContext.Clients
                .Group($"account_{accountNumber}")
                .SendAsync("TransactionCreated", transactionData);
        }

        public async Task NotifyBalanceChangeAsync(string accountNumber, decimal newBalance)
        {
            await _hubContext.Clients
                .Group($"account_{accountNumber}")
                .SendAsync("BalanceUpdated", new { AccountNumber = accountNumber, Balance = newBalance });
        }
    }
}
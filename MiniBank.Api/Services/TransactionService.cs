// Services/TransactionService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniBank.Api.Dtos.Transaction;
using MiniBank.Api.Interfaces;
using MiniBank.Api.Models;

namespace MiniBank.Api.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepo;
        private readonly IAccountRepository _accountRepo;

        private readonly IEmailService _emailService;

        private readonly INotificationService _notificationService;

        public TransactionService(ITransactionRepository transactionRepo, IAccountRepository accountRepo, IEmailService emailService, INotificationService notificationService)
        {
            _transactionRepo = transactionRepo;
            _accountRepo = accountRepo;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<TransactionDto?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepo.GetByIdAsync(id);
            return transaction == null ? null : MapToDto(transaction);
        }

        public async Task<List<TransactionDto>> GetByAccountNumberAsync(string accountNumber)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return new List<TransactionDto>();

            var transactions = await _transactionRepo.GetByAccountIdAsync(account.Id);
            return transactions.Select(MapToDto).ToList();
        }

        public async Task<List<TransactionDto>> GetByDateRangeAsync(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return new List<TransactionDto>();

            var transactions = await _transactionRepo.GetByDateRangeAsync(account.Id, startDate, endDate);
            return transactions.Select(MapToDto).ToList();
        }

        public async Task<TransactionDto> DepositAsync(DepositDto depositDto)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(depositDto.AccountNumber);
            if (account == null)
                throw new Exception("Account not found");

            // Update balance
            account.Balance += depositDto.Amount;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.UpdateAsync(account.Id, account);

            // Create transaction (both from and to same account)
            var transaction = new Transaction
            {
                Amount = depositDto.Amount,
                Description = depositDto.Description,
                Status = "Completed",
                TransactionType = "Deposit",
                TransactionDate = DateTime.UtcNow,
                FromAccountId = account.Id,
                ToAccountId = account.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTransaction = await _transactionRepo.CreateAsync(transaction);
            var transactionDto = MapToDto(createdTransaction);
            await _notificationService.NotifyTransactionAsync(account.AccountNumber, transactionDto);
            await _notificationService.NotifyBalanceChangeAsync(account.AccountNumber, account.Balance);

            _ = _emailService.SendTransactionNotificationAsync(
                toEmail: account.User.Email!,
                transactionType: "Deposit",
                amount: depositDto.Amount,
                accountNumber: depositDto.AccountNumber
            );
            return transactionDto;
        }

        public async Task<TransactionDto> WithdrawAsync(WithdrawDto withdrawDto)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(withdrawDto.AccountNumber);
            if (account == null)
                throw new Exception("Account not found");

            // Check sufficient balance
            if (account.Balance < withdrawDto.Amount)
                throw new Exception("Insufficient balance");

            // Update balance
            account.Balance -= withdrawDto.Amount;
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.UpdateAsync(account.Id, account);

            // Create transaction (both from and to same account)
            var transaction = new Transaction
            {
                Amount = withdrawDto.Amount,
                Description = withdrawDto.Description,
                Status = "Completed",
                TransactionType = "Withdrawal",
                TransactionDate = DateTime.UtcNow,
                FromAccountId = account.Id,
                ToAccountId = account.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTransaction = await _transactionRepo.CreateAsync(transaction);
            _ = _emailService.SendTransactionNotificationAsync(
                toEmail: account.User.Email!,
                transactionType: "Withdrawal",
                amount: withdrawDto.Amount,
                accountNumber: withdrawDto.AccountNumber
            );
            return MapToDto(createdTransaction);
        }

        public async Task<TransactionDto> TransferAsync(TransferDto transferDto)
        {
            var fromAccount = await _accountRepo.GetByAccountNumberAsync(transferDto.FromAccountNumber);
            if (fromAccount == null)
                throw new Exception("Source account not found");

            var toAccount = await _accountRepo.GetByAccountNumberAsync(transferDto.ToAccountNumber);
            if (toAccount == null)
                throw new Exception("Destination account not found");

            // Check same account
            if (fromAccount.Id == toAccount.Id)
                throw new Exception("Cannot transfer to the same account");

            // Check sufficient balance
            if (fromAccount.Balance < transferDto.Amount)
                throw new Exception("Insufficient balance");

            // Update balances
            fromAccount.Balance -= transferDto.Amount;
            fromAccount.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.UpdateAsync(fromAccount.Id, fromAccount);

            toAccount.Balance += transferDto.Amount;
            toAccount.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.UpdateAsync(toAccount.Id, toAccount);

            // Create transaction
            var transaction = new Transaction
            {
                Amount = transferDto.Amount,
                Description = transferDto.Description,
                Status = "Completed",
                TransactionType = "Transfer",
                TransactionDate = DateTime.UtcNow,
                FromAccountId = fromAccount.Id,
                ToAccountId = toAccount.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdTransaction = await _transactionRepo.CreateAsync(transaction);
            _ = _emailService.SendTransactionNotificationAsync(
                toEmail: fromAccount.User.Email!,
                transactionType: "Transfer Sent",
                amount: transferDto.Amount,
                accountNumber: transferDto.FromAccountNumber
            );
            return MapToDto(createdTransaction);
        }

        public async Task<List<TransactionDto>> GetAllAsync()
        {
            var transactions = await _transactionRepo.GetAllAsync();
            return transactions.Select(MapToDto).ToList();
        }

        private TransactionDto MapToDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.id,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Status = transaction.Status,
                TransactionType = transaction.TransactionType,
                TransactionDate = transaction.TransactionDate,
                FromAccountId = transaction.FromAccountId,
                FromAccountNumber = transaction.FromAccount?.AccountNumber ?? "",
                ToAccountId = transaction.ToAccountId,
                ToAccountNumber = transaction.ToAccount?.AccountNumber ?? ""
            };
        }
    }
}
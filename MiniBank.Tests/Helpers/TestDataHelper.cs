using MiniBank.Api.Dtos.Account;
using MiniBank.Api.Dtos.Transaction;
using MiniBank.Api.Models;

namespace MiniBank.Tests.Helpers
{
    public static class TestDataHelper
    {
        public static AppUser CreateTestUser(
            string id = "test-user-id",
            string email = "test@example.com"
        )
        {
            return new AppUser
            {
                Id = id,
                UserName = "testuser",
                Email = email,
                EmailConfirmed = true,
            };
        }

        // For Repository tests - NO User navigation property
        public static Account CreateTestAccount(string userId = "test-user-id", int id = 1)
        {
            return new Account
            {
                Id = id,
                AccountNumber = $"ACC-20260102-{id:D4}",
                Balance = 1000.00m,
                AccountType = "Checking",
                Currency = "ZAR",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        // For Service tests - WITH User navigation property
        public static Account CreateTestAccountWithUser(
            string userId = "test-user-id",
            int id = 1,
            string email = "test@example.com"
        )
        {
            return new Account
            {
                Id = id,
                AccountNumber = $"ACC-20260102-{id:D4}",
                Balance = 1000.00m,
                AccountType = "Checking",
                Currency = "ZAR",
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                User = new AppUser
                {
                    Id = userId,
                    UserName = "testuser",
                    Email = email,
                    EmailConfirmed = true,
                },
            };
        }

        public static Transaction CreateTestTransaction(int accountId, int id = 1)
        {
            return new Transaction
            {
                id = id,
                Amount = 500.00m,
                Description = "Test transaction",
                Status = "Completed",
                TransactionType = "Deposit",
                FromAccountId = accountId,
                ToAccountId = accountId,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        public static CreateAccountDto CreateAccountDto(string userId = "test-user-id")
        {
            return new CreateAccountDto
            {
                UserId = userId,
                AccountType = "Checking",
                Currency = "ZAR",
            };
        }

        public static DepositDto CreateDepositDto(string accountNumber = "ACC-20260102-0001")
        {
            return new DepositDto
            {
                AccountNumber = accountNumber,
                Amount = 500.00m,
                Description = "Test deposit",
            };
        }

        public static WithdrawDto CreateWithdrawDto(string accountNumber = "ACC-20260102-0001")
        {
            return new WithdrawDto
            {
                AccountNumber = accountNumber,
                Amount = 200.00m,
                Description = "Test withdrawal",
            };
        }

        public static TransferDto CreateTransferDto(
            string fromAccount = "ACC-20260102-0001",
            string toAccount = "ACC-20260102-0002"
        )
        {
            return new TransferDto
            {
                FromAccountNumber = fromAccount,
                ToAccountNumber = toAccount,
                Amount = 300.00m,
                Description = "Test transfer",
            };
        }
    }
}

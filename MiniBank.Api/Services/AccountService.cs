// Services/AccountService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniBank.Api.Dtos.Account;
using MiniBank.Api.Interfaces;
using MiniBank.Api.Models;

namespace MiniBank.Api.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepo;

        public AccountService(IAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        public async Task<AccountDto?> GetByIdAsync(int id)
        {
            var account = await _accountRepo.GetByIdAsync(id);
            return account == null ? null : MapToDto(account);
        }

        public async Task<AccountDto?> GetByAccountNumberAsync(string accountNumber)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            return account == null ? null : MapToDto(account);
        }

        public async Task<List<AccountDto>> GetByUserIdAsync(string userId)
        {
            var accounts = await _accountRepo.GetByUserIdAsync(userId);
            return accounts.Select(MapToDto).ToList();
        }

        public async Task<AccountDto> CreateAsync(CreateAccountDto createDto)
        {
            // Generate unique account number
            var accountNumber = GenerateAccountNumber();
            
            // Check if account number already exists (very unlikely but safe)
            while (await _accountRepo.AccountExistsAsync(accountNumber))
            {
                accountNumber = GenerateAccountNumber();
            }

            var account = new Account
            {
                AccountNumber = accountNumber,
                Balance = 0,
                AccountType = createDto.AccountType,
                Currency = createDto.Currency,
                UserId = createDto.UserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdAccount = await _accountRepo.CreateAsync(account);
            return MapToDto(createdAccount);
        }

        public async Task<AccountDto?> UpdateAsync(int id, UpdateAccountDto updateDto)
        {
            var account = await _accountRepo.GetByIdAsync(id);
            if (account == null)
                return null;

            if (updateDto.AccountType != null)
                account.AccountType = updateDto.AccountType;
            
            if (updateDto.IsActive.HasValue)
                account.IsActive = updateDto.IsActive.Value;

            account.UpdatedAt = DateTime.UtcNow;

            var updatedAccount = await _accountRepo.UpdateAsync(id, account);
            return updatedAccount == null ? null : MapToDto(updatedAccount);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var account = await _accountRepo.DeleteAsync(id);
            return account != null;
        }

        public async Task<decimal> GetBalanceAsync(string accountNumber)
        {
            var account = await _accountRepo.GetByAccountNumberAsync(accountNumber);
            return account?.Balance ?? 0;
        }

        private AccountDto MapToDto(Account account)
        {
            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                AccountType = account.AccountType,
                Currency = account.Currency,
                IsActive = account.IsActive,
                UserId = account.UserId,
                CreatedAt = account.CreatedAt
            };
        }

        private string GenerateAccountNumber()
        {
            // Format: ACC-YYYYMMDD-XXXX (e.g., ACC-20260102-1234)
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = new Random().Next(1000, 9999);
            return $"ACC-{datePart}-{randomPart}";
        }
    }
}
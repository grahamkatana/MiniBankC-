using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniBank.Api.Models;

namespace MiniBank.Api.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByAccountNumberAsync(string accountNumber);
        Task<List<Account>> GetByUserIdAsync(string userId);
        Task<Account> CreateAsync(Account account);
        Task<Account?> UpdateAsync(int id, Account account);
        Task<Account?> DeleteAsync(int id);
        Task<bool> AccountExistsAsync(string accountNumber);
        Task<decimal> GetBalanceAsync(int accountId);
    }
}
// Interfaces/IAccountService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniBank.Api.Dtos.Account;

namespace MiniBank.Api.Interfaces
{
    public interface IAccountService
    {
        Task<AccountDto?> GetByIdAsync(int id);
        Task<AccountDto?> GetByAccountNumberAsync(string accountNumber);
        Task<List<AccountDto>> GetByUserIdAsync(string userId);
        Task<AccountDto> CreateAsync(CreateAccountDto createDto);
        Task<AccountDto?> UpdateAsync(int id, UpdateAccountDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<decimal> GetBalanceAsync(string accountNumber);
    }
}
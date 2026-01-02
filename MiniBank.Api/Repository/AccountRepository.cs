using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniBank.Api.Interfaces;
using MiniBank.Api.Models;
using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;

namespace MiniBank.Api.Repository
{
    public class AccountRepository: IAccountRepository
    {
       private readonly ApplicationDBContext _context;

        public AccountRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public Task<Account?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Account?> GetByAccountNumberAsync(string accountNumber)
        {
            throw new NotImplementedException();
        }

        public Task<List<Account>> GetByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Account> CreateAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public Task<Account?> UpdateAsync(int id, Account account)
        {
            throw new NotImplementedException();
        }

        public Task<Account?> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AccountExistsAsync(string accountNumber)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetBalanceAsync(int accountId)
        {
            throw new NotImplementedException();
        }
        
    }
}
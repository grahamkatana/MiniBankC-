using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniBank.Api.Models;

namespace MiniBank.Api.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<List<Transaction>> GetByAccountIdAsync(int accountId);
        Task<List<Transaction>> GetByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task<Transaction?> UpdateStatusAsync(int id, string status);
        Task<List<Transaction>> GetAllAsync();
        
    }
}
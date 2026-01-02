using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniBank.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniBank.Api.Data;
using MiniBank.Api.Models;

namespace MiniBank.Api.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDBContext _context;
        public TransactionRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public Task<Transaction> CreateAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<List<Transaction>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Transaction>> GetByAccountIdAsync(int accountId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Transaction>> GetByDateRangeAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction?> UpdateStatusAsync(int id, string status)
        {
            throw new NotImplementedException();
        }
    }
}
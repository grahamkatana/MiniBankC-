using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniBank.Api.Dtos.Transaction
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public int FromAccountId { get; set; }
        public string FromAccountNumber { get; set; } = string.Empty;
        public int ToAccountId { get; set; }
        public string ToAccountNumber { get; set; } = string.Empty;
        
    }
}
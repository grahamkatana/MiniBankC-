using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniBank.Api.Dtos.Account
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
    }
}
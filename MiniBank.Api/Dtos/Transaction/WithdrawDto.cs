using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MiniBank.Api.Dtos.Transaction
{
    public class WithdrawDto
    {
        [Required]
        public string AccountNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(0.05, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        public string Description { get; set; } = "Withdrawal";
    }
}
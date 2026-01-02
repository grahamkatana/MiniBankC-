using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MiniBank.Api.Dtos.Transaction
{
    public class TransferDto
    {
        [Required]
        public string FromAccountNumber { get; set; } = string.Empty;
        
        [Required]
        public string ToAccountNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        public string Description { get; set; } = "Transfer";
        
    }
}
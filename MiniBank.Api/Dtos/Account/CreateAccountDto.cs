using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MiniBank.Api.Dtos.Account
{
    public class CreateAccountDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string AccountType { get; set; } = "Checking";
        
        public string Currency { get; set; } = "ZAR";
    }
}
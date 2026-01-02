using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniBank.Api.Dtos.Account
{
    public class UpdateAccountDto
    {
        public string? AccountType { get; set; }
        public bool? IsActive { get; set; }
    }
}
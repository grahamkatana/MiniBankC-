
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniBank.Api.Models
{
    [Table("Transactions")]
    public class Transaction
    {
        public int id { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        // status should be an enum for 'Pending', 'Completed', 'Failed'
        public string Status { get; set; } = "Completed";
        // transaction type should be an enum for 'Deposit', 'Withdrawal', 'Transfer'
        public string TransactionType { get; set; } = "Transfer";
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        public int FromAccountId { get; set; }
        public Account FromAccount { get; set; } = null!;
        public int ToAccountId { get; set; }
        public Account ToAccount { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        
    }
}
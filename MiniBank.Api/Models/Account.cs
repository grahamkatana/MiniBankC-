using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
namespace MiniBank.Api.Models
{
    [Table("Accounts")]
    public class Account
    {
        public int Id { get; set; }
        // annottate account number as unique
        [Column(TypeName = "nvarchar(20)")]
        public string AccountNumber { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
        public string AccountType { get; set; } = "Checking";
        public string Currency { get; set; } = "ZAR";

        public bool IsActive { get; set; } = true;
        public AppUser User { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
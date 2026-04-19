using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashPay.API.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal LockedBalance { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalReceived { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalWithdrawn { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PendingAmount { get; set; } = 0;

        public string Currency { get; set; } = "PKR";
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Concurrency Token: To prevent balance mismatch during high traffic
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public ICollection<Ledger> LedgerEntries { get; set; } = new List<Ledger>();
    }
}
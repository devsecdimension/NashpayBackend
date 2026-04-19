using System.ComponentModel.DataAnnotations.Schema;

namespace NashPay.API.Models
{
    public class Settlement
    {
        public int Id { get; set; }
        public string SettlementId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        public string Currency { get; set; } = "PKR";
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionPercentage { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }

        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpectedSettlementDate { get; set; }
        public DateTime? ActualSettlementDate { get; set; }

        public string? TransactionReference { get; set; }
        public string? FailureReason { get; set; }
        public string? Notes { get; set; }
    }
}
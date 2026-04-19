using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashPay.API.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; } = "Customer"; 
        public string KYCStatus { get; set; } = "Pending";

        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? TaxId { get; set; }
        public string? Website { get; set; }
        public string? BusinessDescription { get; set; }

        public BankDetails? BankDetails { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CommissionPercentage { get; set; } = 0m;

        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Pakistan";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }

        // Navigation Properties with InverseProperty or Explicit Mapping
        public Wallet Wallet { get; set; }
        
        [InverseProperty("Sender")]
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        
        [InverseProperty("Receiver")]
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();

        public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
        public ICollection<Ledger> LedgerEntries { get; set; } = new List<Ledger>();
        public ICollection<KYCDocument> KYCDocuments { get; set; } = new List<KYCDocument>();
        public ICollection<WebhookLog> WebhookLogs { get; set; } = new List<WebhookLog>();
        public ICollection<FraudAlert> FraudAlerts { get; set; } = new List<FraudAlert>();
    }
}
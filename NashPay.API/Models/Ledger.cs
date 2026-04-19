using System;

namespace NashPay.API.Models
{
    public class Ledger
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        
        public decimal DebitAmount { get; set; } = 0; // Money Out
        public decimal CreditAmount { get; set; } = 0; // Money In
        public decimal BalanceAfter { get; set; } = 0;
        
        public string TransactionType { get; set; } // Payment, Refund, Withdrawal, Settlement
        public string Description { get; set; }
        public int? TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        
        public string Status { get; set; } = "Completed"; // Pending, Completed, Failed
        public bool IsLocked { get; set; } = false; // For T+3 settlements
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

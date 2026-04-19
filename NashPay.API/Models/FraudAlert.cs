using System;

namespace NashPay.API.Models
{
    public class FraudAlert
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        
        public string AlertType { get; set; } // HighFrequency, SuspiciousPattern, LargeAmount, etc
        public string Description { get; set; }
        public decimal SuspiciousAmount { get; set; }
        
        public string Status { get; set; } = "Open"; // Open, Investigating, Resolved, FalsePositive
        public string Resolution { get; set; }
        
        public int? RelatedTransactionId { get; set; }
        public Transaction RelatedTransaction { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public string ResolvedBy { get; set; } // Admin user ID
    }
}

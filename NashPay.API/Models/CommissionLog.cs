using System;

namespace NashPay.API.Models
{
    public class CommissionLog
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        
        public string MerchantId { get; set; }
        public User Merchant { get; set; }
        
        public string AdminId { get; set; }
        public User Admin { get; set; }
        
        public decimal TransactionAmount { get; set; }
        public decimal CommissionPercentage { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal MerchantAmount { get; set; }  // Amount merchant gets
        public decimal AdminAmount { get; set; }     // Amount admin gets/keeps
        
        public string Status { get; set; } = "Pending"; // Pending, Settled, Refunded
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SettledAt { get; set; }
    }
}

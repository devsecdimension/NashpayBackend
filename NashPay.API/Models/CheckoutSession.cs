using System;

namespace NashPay.API.Models
{
    public class CheckoutSession
    {
        public int Id { get; set; }
        public string CheckoutSessionId { get; set; } // Unique session ID
        public string MerchantId { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PKR";
        
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        
        public string Status { get; set; } = "pending"; // pending, completed, cancelled, expired
        public string TransactionId { get; set; } // Link to transaction once payment made
        
        public string Metadata { get; set; } // JSON metadata
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } // Default 24 hours
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        // Navigation property
        public User Merchant { get; set; }
    }
}

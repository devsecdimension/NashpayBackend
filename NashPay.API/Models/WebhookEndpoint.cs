using System;
using System.Collections.Generic;

namespace NashPay.API.Models
{
    public class WebhookEndpoint
    {
        public int Id { get; set; }
        public string WebhookId { get; set; } // Unique webhook ID
        public string UserId { get; set; }
        public User User { get; set; }
        
        public string Url { get; set; }
        public string Events { get; set; } // Comma-separated: payment.completed,settlement.completed
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Security
        public string Secret { get; set; } // Used to sign webhooks (HMAC-SHA256)
        
        // Statistics
        public int SuccessfulDeliveries { get; set; } = 0;
        public int FailedDeliveries { get; set; } = 0;
        public DateTime? LastDeliveryAt { get; set; }
        public DateTime? LastFailureAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public ICollection<WebhookLog> Logs { get; set; } = new List<WebhookLog>();
    }
}

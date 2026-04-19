using System;

namespace NashPay.API.Models
{
    public class WebhookLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        
        // Optional link to the endpoint this log is related to
        public int? WebhookEndpointId { get; set; }
        public WebhookEndpoint? WebhookEndpoint { get; set; }
        
        public string WebhookUrl { get; set; }
        public string EventType { get; set; } // payment.success, payment.failed, settlement.completed, etc
        public string Payload { get; set; } // JSON payload sent
        public int HttpStatusCode { get; set; }
        public string ResponseBody { get; set; }
        
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        
        public int RetryCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

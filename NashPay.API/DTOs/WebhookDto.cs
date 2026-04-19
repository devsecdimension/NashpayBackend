using System;
using System.Collections.Generic;

namespace NashPay.API.DTOs
{
    // Request DTOs
    public class RegisterWebhookDto
    {
        public string Url { get; set; }
        public List<string> Events { get; set; } = new List<string>(); // payment.completed, settlement.completed, etc
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWebhookDto
    {
        public string Url { get; set; }
        public List<string> Events { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class WebhookEventDto
    {
        public string EventId { get; set; }
        public string EventType { get; set; }
        public string ResourceId { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class WebhookPayloadDto
    {
        public string EventId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public string Signature { get; set; } // HMAC-SHA256
    }

    // Response DTOs
    public class WebhookDetailDto
    {
        public string WebhookId { get; set; }
        public string Url { get; set; }
        public List<string> Events { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public DateTime? LastDeliveryAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WebhookLogDetailDto
    {
        public string LogId { get; set; }
        public string WebhookUrl { get; set; }
        public string EventType { get; set; }
        public string Payload { get; set; }
        public int HttpStatusCode { get; set; }
        public string ResponseBody { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? NextRetryAt { get; set; }
    }

    public class WebhookTestDto
    {
        public string EventType { get; set; }
        public Dictionary<string, object> TestData { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace NashPay.API.DTOs
{
    public class InitiateCheckoutDto
    {
        [Required]
        public string OrderId { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string ProductName { get; set; }

        public string? ProductDescription { get; set; }

        [Required, Url]
        public string ReturnUrl { get; set; }

        [Required, Url]
        public string CancelUrl { get; set; }

        [Required, EmailAddress]
        public string CustomerEmail { get; set; }

        public string? CustomerName { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class CheckoutResponseDto
    {
        public string CheckoutSessionId { get; set; }
        public string CheckoutUrl { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class CheckoutSessionDto
    {
        public string CheckoutSessionId { get; set; }
        public string MerchantId { get; set; }
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    // missing work for controller and service file for checkout verification
    public class VerifyCheckoutDto
    {
        [Required]
        public string TransactionId { get; set; }
    }
}
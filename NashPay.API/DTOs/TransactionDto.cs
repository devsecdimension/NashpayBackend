using System.ComponentModel.DataAnnotations;

namespace NashPay.API.DTOs
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public string SenderId { get; set; }
        public string? ReceiverId { get; set; } // Nullable rakha hai safety ke liye
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public decimal NetAmount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public bool IsLive { get; set; }
        
        // --- Missing Fields that connected to Admin service ---
        public string PaymentMethod { get; set; } 
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
        // ---------------------------------------

        public DateTime CreatedAt { get; set; }
    }

    public class InitiatePaymentDto
    {
        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string PaymentMethod { get; set; } 

        [Required, EmailAddress]
        public string ReceiverEmail { get; set; }

        public bool IsLive { get; set; } = false;
    }
}

public class PaymentResponseDto
{
    public string TransactionId { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal NetAmount { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}
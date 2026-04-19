using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashPay.API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        
        [Required]
        public string TransactionId { get; set; } 

        public string SenderId { get; set; } 
        public string? ReceiverId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Fee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount { get; set; }

        public string Currency { get; set; } = "PKR";
        public string Status { get; set; } = "Pending"; 
        public string PaymentMethod { get; set; } = "Card";
        public string Type { get; set; } = "Payment"; 
        public bool IsLive { get; set; } = false; 
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key Mapping
        [ForeignKey("SenderId")]
        public User Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public User? Receiver { get; set; }
    }
}
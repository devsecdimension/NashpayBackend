using System;

namespace NashPay.API.Models
{
    public class BankDetails
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        
        public string BankName { get; set; }
        public string AccountNumber { get; set; }  // Can be masked in response
        public string AccountHolderName { get; set; }
        public string AccountType { get; set; } // Business Current, Business Savings, etc
        public string IBAN { get; set; }
        public string BranchCode { get; set; }
        
        public bool IsVerified { get; set; } = false;
        public bool IsPrimary { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}

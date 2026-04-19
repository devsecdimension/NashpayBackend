using System;

namespace NashPay.API.Models
{
    public class KYCDocument
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        
        public string DocumentType { get; set; } // NationalID, Passport, BusinessRegistration, TaxCertificate, AddressProof
        public string DocumentUrl { get; set; }
        public string FileName { get; set; }
        
        public string Status { get; set; } = "Pending"; // Pending, Verified, Rejected
        public string RejectionReason { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }
        
        public string VerifiedBy { get; set; } // Admin user ID who verified
        public string Notes { get; set; }
    }
}

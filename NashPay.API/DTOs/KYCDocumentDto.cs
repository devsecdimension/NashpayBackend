using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace NashPay.API.DTOs
{
    public class KYCDocumentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string DocumentType { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public class UploadKYCDocumentDto
    {
        [Required]
        public string DocumentType { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }

    public class VerifyKYCDocumentDto
    {
        [Required]
        public int DocumentId { get; set; }
        [Required]
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
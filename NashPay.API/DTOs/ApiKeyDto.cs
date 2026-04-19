using System.ComponentModel.DataAnnotations;

namespace NashPay.API.DTOs
{
    public class ApiKeyDto
    {
        public int Id { get; set; }
        public string PublicKey { get; set; }
        
        // added missing field for secret key
        public string SecretKey { get; set; } 
        
        public string KeyName { get; set; }
        public string Environment { get; set; } // Test or Live
        public bool IsActive { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateApiKeyDto
    {
        [Required]
        public string KeyName { get; set; }
        [Required]
        public string Environment { get; set; } = "Test";
    }

    public class ApiKeyResponseDto
    {
        public string PublicKey { get; set; }
        public string SecretKey { get; set; } // Only shown once after creation
        public string Message { get; set; }
    }
}
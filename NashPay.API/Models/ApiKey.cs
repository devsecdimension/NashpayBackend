using System;

namespace NashPay.API.Models
{
    public class ApiKey
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        
        public string PublicKey { get; set; }
        public string SecretKey { get; set; }
        public string KeyName { get; set; } = "API Key";
        public string Environment { get; set; } = "Test"; // Test or Live
        
        public bool IsActive { get; set; } = true;
        public DateTime? LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        
        // Rate Limiting
        public int RequestsPerMinute { get; set; } = 60;
    }
}

namespace NashPay.API.DTOs
{
    public class FraudAlertDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AlertType { get; set; }
        public string Description { get; set; }
        public decimal SuspiciousAmount { get; set; }
        public string Status { get; set; }
        public string Resolution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class ResolveFraudAlertDto
    {
        public int AlertId { get; set; }
        public string Resolution { get; set; }
    }
}

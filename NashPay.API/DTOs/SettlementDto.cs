namespace NashPay.API.DTOs
{
    public class SettlementDto
    {
        public int Id { get; set; }
        public string SettlementId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public DateTime InitiatedAt { get; set; }
        public DateTime ExpectedSettlementDate { get; set; }
        public DateTime? ActualSettlementDate { get; set; }
    }

    public class InitiateSettlementDto
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
    }

    public class SettlementResponseDto
    {
        public string SettlementId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpectedSettlementDate { get; set; }
        public string Message { get; set; }
    }
}

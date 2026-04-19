namespace NashPay.API.DTOs
{
    public class LedgerDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LedgerEntryDto
    {
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
    }
}

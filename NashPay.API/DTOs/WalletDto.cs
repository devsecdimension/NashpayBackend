namespace NashPay.API.DTOs
{
    public class WalletDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal LockedBalance { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class WalletBalanceDto
    {
        public decimal Balance { get; set; }
        public decimal LockedBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; }
    }
}

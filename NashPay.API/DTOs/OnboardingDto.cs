namespace NashPay.API.DTOs
{
    // Enhanced Registration DTO with KYC info
    public class EnhancedRegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; } // Merchant or Customer (Admin registration separate)
        
        // Business Info (for Merchants)
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string RegistrationNumber { get; set; }
        public string TaxId { get; set; }
        public string Website { get; set; }
        public string BusinessDescription { get; set; }
        
        // Address
        public string Street { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        
        // KYC Documents - file uploads
        public List<KYCDocumentUploadDto> KYCDocuments { get; set; } = new();
    }
    
    // KYC Document Upload DTO
    public class KYCDocumentUploadDto
    {
        public string DocumentType { get; set; } // national_id, passport, business_registration, etc
        public IFormFile File { get; set; }
        public string DocumentNumber { get; set; }
    }
    
    // Bank Details DTO
    public class BankDetailsDto
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string MaskedAccountNumber { get; set; } // For display: ****7823
        public string AccountHolderName { get; set; }
        public string AccountType { get; set; }
        public string IBAN { get; set; }
        public string BranchCode { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
    
    // Create/Update Bank Details DTO
    public class CreateBankDetailsDto
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountType { get; set; }
        public string IBAN { get; set; }
        public string BranchCode { get; set; }
    }
    
    // Commission Details DTO
    public class CommissionDetailsDto
    {
        public int Id { get; set; }
        public string MerchantId { get; set; }
        public string MerchantName { get; set; }
        public decimal CommissionPercentage { get; set; }
        public decimal TotalTransactionAmount { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Pending { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
    // Admin Commission Configuration DTO
    public class AdminCommissionConfigDto
    {
        public string AdminId { get; set; }
        public decimal DefaultCommissionPercentage { get; set; }
        public List<CommissionDetailsDto> MerchantCommissions { get; set; } = new();
    }
    
    // Merchant Commission Summary DTO
    public class MerchantCommissionSummaryDto
    {
        public decimal CommissionPercentage { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalCommissionDeducted { get; set; }
        public decimal NetEarnings { get; set; }
        public List<CommissionLogDto> RecentTransactions { get; set; } = new();
    }
    
    // Commission Log DTO
    public class CommissionLogDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal CommissionPercentage { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal MerchantAmount { get; set; }
        public decimal AdminAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SettledAt { get; set; }
    }
    
    // Set Commission DTO
    public class SetCommissionDto
    {
        public decimal CommissionPercentage { get; set; }
    }
    
    // Calculate Commission DTO
    public class CalculateCommissionDto
    {
        public decimal Amount { get; set; }
        public decimal CommissionPercentage { get; set; }
    }
}

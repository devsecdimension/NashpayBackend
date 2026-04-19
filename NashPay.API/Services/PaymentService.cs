using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using NashPay.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace NashPay.API.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> InitiatePaymentAsync(string senderId, InitiatePaymentDto model);
        Task<TransactionDto> GetTransactionAsync(string transactionId);
        Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<PaymentResponseDto> VerifyPaymentAsync(string transactionId);
        Task<bool> RefundTransactionAsync(string transactionId, string reason);
    }

    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IWalletService _walletService;

        public PaymentService(AppDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(string senderId, InitiatePaymentDto model)
        {
            try
            {
                // Validate sender wallet exists
                var senderWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == senderId);
                if (senderWallet == null)
                    throw new Exception("Sender wallet not found");

                // Calculate fee (1% of transaction)
                decimal fee = model.Amount * 0.01m;
                decimal netAmount = model.Amount - fee;

                // Generate unique transaction ID
                string transactionId = $"TXN_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                var transaction = new Transaction
                {
                    TransactionId = transactionId,
                    SenderId = senderId,
                    ReceiverId = model.ReceiverEmail, // Will be resolved to actual user ID in real scenario
                    Amount = model.Amount,
                    Fee = fee,
                    NetAmount = netAmount,
                    Currency = "PKR",
                    Status = "Pending",
                    PaymentMethod = model.PaymentMethod,
                    IsLive = model.IsLive,
                    Description = model.Description,
                    Type = "Payment",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);

                // Create ledger entry for payment
                var ledgerEntry = new Ledger
                {
                    UserId = senderId,
                    DebitAmount = model.Amount,
                    BalanceAfter = senderWallet.Balance - model.Amount,
                    TransactionType = "Payment",
                    Description = model.Description,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Ledgers.Add(ledgerEntry);
                await _context.SaveChangesAsync();

                // Create ledger entry for payment
                ledgerEntry.TransactionId = transaction.Id;
                await _context.SaveChangesAsync();

                return new PaymentResponseDto
                {
                    TransactionId = transactionId,
                    Status = "Pending",
                    Amount = model.Amount,
                    Fee = fee,
                    NetAmount = netAmount,
                    Message = "Payment initiated successfully",
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Payment initiation failed: {ex.Message}");
            }
        }

        public async Task<TransactionDto> GetTransactionAsync(string transactionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
                throw new Exception("Transaction not found");

            return MapToTransactionDto(transaction);
        }

        public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            var transactions = await _context.Transactions
                .Where(t => t.SenderId == userId || t.ReceiverId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return transactions.Select(MapToTransactionDto);
        }

        public async Task<PaymentResponseDto> VerifyPaymentAsync(string transactionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
                throw new Exception("Transaction not found");

            // Update transaction status
            transaction.Status = "Completed";
            transaction.UpdatedAt = DateTime.UtcNow;

            // Update wallet balances
            var senderWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == transaction.SenderId);
            if (senderWallet != null)
            {
                senderWallet.Balance -= transaction.Amount;
                senderWallet.TotalWithdrawn += transaction.Amount;
                senderWallet.LastUpdated = DateTime.UtcNow;
            }

            _context.Transactions.Update(transaction);
            _context.Wallets.Update(senderWallet);
            await _context.SaveChangesAsync();

            return new PaymentResponseDto
            {
                TransactionId = transactionId,
                Status = "Completed",
                Amount = transaction.Amount,
                Fee = transaction.Fee,
                NetAmount = transaction.NetAmount,
                Message = "Payment verified successfully",
                CreatedAt = transaction.CreatedAt
            };
        }

        public async Task<bool> RefundTransactionAsync(string transactionId, string reason)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null)
                throw new Exception("Transaction not found");

            if (transaction.Status == "Refunded")
                throw new Exception("Transaction already refunded");

            // Create refund transaction
            var refundTransaction = new Transaction
            {
                TransactionId = $"REFUND_{transactionId}",
                SenderId = transaction.ReceiverId,
                ReceiverId = transaction.SenderId,
                Amount = transaction.Amount,
                Fee = 0,
                NetAmount = transaction.Amount,
                Currency = "PKR",
                Status = "Completed",
                PaymentMethod = transaction.PaymentMethod,
                IsLive = transaction.IsLive,
                Description = $"Refund for {transactionId}: {reason}",
                Type = "Refund",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            transaction.Status = "Refunded";

            _context.Transactions.Add(refundTransaction);
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();

            return true;
        }

        private TransactionDto MapToTransactionDto(Transaction transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                TransactionId = transaction.TransactionId,
                SenderId = transaction.SenderId,
                ReceiverId = transaction.ReceiverId,
                Amount = transaction.Amount,
                Fee = transaction.Fee,
                NetAmount = transaction.NetAmount,
                Currency = transaction.Currency,
                Status = transaction.Status,
                PaymentMethod = transaction.PaymentMethod,
                Type = transaction.Type,
                IsLive = transaction.IsLive,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt
            };
        }
    }
}

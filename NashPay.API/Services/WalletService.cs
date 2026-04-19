using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;

namespace NashPay.API.Services
{
    public interface IWalletService
    {
        Task<WalletDto> GetWalletAsync(string userId);
        Task<WalletBalanceDto> GetBalanceAsync(string userId);
        Task<bool> UpdateBalanceAsync(string userId, decimal amount, string transactionType);
        Task<IEnumerable<LedgerDto>> GetLedgerEntriesAsync(string userId);
    }

    public class WalletService : IWalletService
    {
        private readonly AppDbContext _context;

        public WalletService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WalletDto> GetWalletAsync(string userId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                throw new Exception("Wallet not found");

            return new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                LockedBalance = wallet.LockedBalance,
                PendingAmount = wallet.PendingAmount,
                TotalReceived = wallet.TotalReceived,
                TotalWithdrawn = wallet.TotalWithdrawn,
                Currency = wallet.Currency,
                Status = wallet.Status,
                LastUpdated = wallet.LastUpdated
            };
        }

        public async Task<WalletBalanceDto> GetBalanceAsync(string userId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                throw new Exception("Wallet not found");

            return new WalletBalanceDto
            {
                Balance = wallet.Balance,
                LockedBalance = wallet.LockedBalance,
                AvailableBalance = wallet.Balance - wallet.LockedBalance,
                Currency = wallet.Currency
            };
        }

        public async Task<bool> UpdateBalanceAsync(string userId, decimal amount, string transactionType)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                throw new Exception("Wallet not found");

            // Always create ledger entry first (Audit Trail)
            var ledgerEntry = new Ledger
            {
                UserId = userId,
                TransactionType = transactionType,
                Description = $"{transactionType}: {amount} PKR",
                Status = "Completed"
            };

            if (amount > 0)
            {
                ledgerEntry.CreditAmount = amount;
                wallet.Balance += amount;
                wallet.TotalReceived += amount;
            }
            else
            {
                ledgerEntry.DebitAmount = Math.Abs(amount);
                wallet.Balance -= Math.Abs(amount);
                wallet.TotalWithdrawn += Math.Abs(amount);
            }

            ledgerEntry.BalanceAfter = wallet.Balance;
            wallet.LastUpdated = DateTime.UtcNow;

            _context.Ledgers.Add(ledgerEntry);
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<LedgerDto>> GetLedgerEntriesAsync(string userId)
        {
            var entries = await _context.Ledgers
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .ToListAsync();

            return entries.Select(e => new LedgerDto
            {
                Id = e.Id,
                UserId = e.UserId,
                DebitAmount = e.DebitAmount,
                CreditAmount = e.CreditAmount,
                BalanceAfter = e.BalanceAfter,
                TransactionType = e.TransactionType,
                Description = e.Description,
                Status = e.Status,
                IsLocked = e.IsLocked,
                CreatedAt = e.CreatedAt
            });
        }
    }
}

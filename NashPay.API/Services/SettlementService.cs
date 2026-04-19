using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace NashPay.API.Services
{
    public interface ISettlementService
    {
        Task<SettlementResponseDto> InitiateSettlementAsync(string userId, InitiateSettlementDto model);
        Task<SettlementDto> GetSettlementAsync(string settlementId);
        Task<IEnumerable<SettlementDto>> GetUserSettlementsAsync(string userId);
        Task<bool> UpdateSettlementStatusAsync(string settlementId, string status);
        Task<bool> SimulateT3SettlementAsync(string userId, decimal amount);
    }

    public class SettlementService : ISettlementService
    {
        private readonly AppDbContext _context;

        public SettlementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SettlementResponseDto> InitiateSettlementAsync(string userId, InitiateSettlementDto model)
        {
            try
            {
                // Validate wallet has sufficient balance
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet == null)
                    throw new Exception("Wallet not found");

                if (wallet.Balance < model.Amount)
                    throw new Exception("Insufficient balance for settlement");

                // Generate settlement ID
                string settlementId = $"STL_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                // Calculate T+3 settlement date (3 business days)
                var expectedDate = CalculateT3SettlementDate();

                var settlement = new Settlement
                {
                    SettlementId = settlementId,
                    UserId = userId,
                    Amount = model.Amount,
                    Currency = "PKR",
                    Status = "Pending",
                    BankName = model.BankName,
                    AccountNumber = model.AccountNumber,
                    AccountHolderName = model.AccountHolderName,
                    InitiatedAt = DateTime.UtcNow,
                    ExpectedSettlementDate = expectedDate
                };

                // Lock the balance
                wallet.LockedBalance += model.Amount;
                wallet.PendingAmount += model.Amount;

                _context.Settlements.Add(settlement);
                _context.Wallets.Update(wallet);

                // Create ledger entry
                var ledgerEntry = new Ledger
                {
                    UserId = userId,
                    DebitAmount = model.Amount,
                    BalanceAfter = wallet.Balance - model.Amount,
                    TransactionType = "Settlement",
                    Description = $"Settlement initiated to {model.BankName}",
                    Status = "Pending",
                    IsLocked = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Ledgers.Add(ledgerEntry);
                await _context.SaveChangesAsync();

                return new SettlementResponseDto
                {
                    SettlementId = settlementId,
                    Status = "Pending",
                    Amount = model.Amount,
                    ExpectedSettlementDate = expectedDate,
                    Message = "Settlement initiated successfully"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Settlement initiation failed: {ex.Message}");
            }
        }

        public async Task<SettlementDto> GetSettlementAsync(string settlementId)
        {
            var settlement = await _context.Settlements
                .FirstOrDefaultAsync(s => s.SettlementId == settlementId);

            if (settlement == null)
                throw new Exception("Settlement not found");

            return MapToSettlementDto(settlement);
        }

        public async Task<IEnumerable<SettlementDto>> GetUserSettlementsAsync(string userId)
        {
            var settlements = await _context.Settlements
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.InitiatedAt)
                .ToListAsync();

            return settlements.Select(MapToSettlementDto);
        }

        public async Task<bool> UpdateSettlementStatusAsync(string settlementId, string status)
        {
            var settlement = await _context.Settlements
                .FirstOrDefaultAsync(s => s.SettlementId == settlementId);

            if (settlement == null)
                throw new Exception("Settlement not found");

            settlement.Status = status;

            if (status == "Completed")
            {
                settlement.ActualSettlementDate = DateTime.UtcNow;

                // Unlock the balance
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == settlement.UserId);
                if (wallet != null)
                {
                    wallet.LockedBalance -= settlement.Amount;
                    wallet.Balance -= settlement.Amount;
                    wallet.TotalWithdrawn += settlement.Amount;
                    wallet.PendingAmount -= settlement.Amount;
                    _context.Wallets.Update(wallet);
                }
            }

            _context.Settlements.Update(settlement);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SimulateT3SettlementAsync(string userId, decimal amount)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                throw new Exception("Wallet not found");

            // Create immediate settlement for testing
            string settlementId = $"STL_SIM_{DateTime.UtcNow.Ticks}";

            var settlement = new Settlement
            {
                SettlementId = settlementId,
                UserId = userId,
                Amount = amount,
                Currency = "PKR",
                Status = "Completed",
                BankName = "Test Bank",
                AccountNumber = "Test Account",
                AccountHolderName = wallet.User?.FullName ?? "User",
                InitiatedAt = DateTime.UtcNow,
                ActualSettlementDate = DateTime.UtcNow
            };

            wallet.LockedBalance -= amount;
            wallet.Balance -= amount;
            wallet.TotalWithdrawn += amount;
            wallet.PendingAmount = 0;

            _context.Settlements.Add(settlement);
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            return true;
        }

        private DateTime CalculateT3SettlementDate()
        {
            var date = DateTime.UtcNow.AddDays(3);

            // Skip weekends
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        private SettlementDto MapToSettlementDto(Settlement settlement)
        {
            return new SettlementDto
            {
                Id = settlement.Id,
                SettlementId = settlement.SettlementId,
                UserId = settlement.UserId,
                Amount = settlement.Amount,
                Currency = settlement.Currency,
                Status = settlement.Status,
                BankName = settlement.BankName,
                AccountNumber = settlement.AccountNumber,
                AccountHolderName = settlement.AccountHolderName,
                InitiatedAt = settlement.InitiatedAt,
                ExpectedSettlementDate = settlement.ExpectedSettlementDate,
                ActualSettlementDate = settlement.ActualSettlementDate
            };
        }
    }
}

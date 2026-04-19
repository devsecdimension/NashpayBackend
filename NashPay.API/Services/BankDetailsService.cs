using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace NashPay.API.Services
{
    public interface IBankDetailsService
    {
        Task<BankDetailsDto> AddBankDetailsAsync(string userId, CreateBankDetailsDto model);
        Task<IEnumerable<BankDetailsDto>> GetBankDetailsAsync(string userId);
        Task<BankDetailsDto> GetPrimaryBankDetailsAsync(string userId);
        Task<BankDetailsDto> UpdateBankDetailsAsync(string userId, int bankId, CreateBankDetailsDto model);
        Task<bool> DeleteBankDetailsAsync(string userId, int bankId);
        Task<bool> SetPrimaryBankAsync(string userId, int bankId);
    }

    public class BankDetailsService : IBankDetailsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BankDetailsService> _logger;

        public BankDetailsService(AppDbContext context, ILogger<BankDetailsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BankDetailsDto> AddBankDetailsAsync(string userId, CreateBankDetailsDto model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new Exception("User not found");

                // If this is the first bank account, set as primary
                var existingBanks = await _context.BankDetails
                    .Where(b => b.UserId == userId)
                    .CountAsync();

                var bankDetails = new BankDetails
                {
                    UserId = userId,
                    BankName = model.BankName,
                    AccountNumber = model.AccountNumber,
                    AccountHolderName = model.AccountHolderName,
                    AccountType = model.AccountType,
                    IBAN = model.IBAN,
                    BranchCode = model.BranchCode,
                    IsPrimary = existingBanks == 0, // First account is primary
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BankDetails.Add(bankDetails);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bank details added for user {userId}");
                return MapToDto(bankDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding bank details");
                throw;
            }
        }

        public async Task<IEnumerable<BankDetailsDto>> GetBankDetailsAsync(string userId)
        {
            var bankDetails = await _context.BankDetails
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.IsPrimary)
                .ToListAsync();

            return bankDetails.Select(MapToDto);
        }

        public async Task<BankDetailsDto> GetPrimaryBankDetailsAsync(string userId)
        {
            var primary = await _context.BankDetails
                .FirstOrDefaultAsync(b => b.UserId == userId && b.IsPrimary);

            if (primary == null)
                throw new Exception("No primary bank account configured");

            return MapToDto(primary);
        }

        public async Task<BankDetailsDto> UpdateBankDetailsAsync(string userId, int bankId, CreateBankDetailsDto model)
        {
            try
            {
                var bankDetails = await _context.BankDetails
                    .FirstOrDefaultAsync(b => b.Id == bankId && b.UserId == userId);

                if (bankDetails == null)
                    throw new Exception("Bank account not found");

                bankDetails.BankName = model.BankName;
                bankDetails.AccountNumber = model.AccountNumber;
                bankDetails.AccountHolderName = model.AccountHolderName;
                bankDetails.AccountType = model.AccountType;
                bankDetails.IBAN = model.IBAN;
                bankDetails.BranchCode = model.BranchCode;
                bankDetails.UpdatedAt = DateTime.UtcNow;
                bankDetails.IsVerified = false; // Reset verification when updated

                _context.BankDetails.Update(bankDetails);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bank details updated for user {userId}");
                return MapToDto(bankDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bank details");
                throw;
            }
        }

        public async Task<bool> DeleteBankDetailsAsync(string userId, int bankId)
        {
            try
            {
                var bankDetails = await _context.BankDetails
                    .FirstOrDefaultAsync(b => b.Id == bankId && b.UserId == userId);

                if (bankDetails == null)
                    throw new Exception("Bank account not found");

                // Don't allow deleting if it's the primary account and only one exists
                var count = await _context.BankDetails
                    .Where(b => b.UserId == userId)
                    .CountAsync();

                if (bankDetails.IsPrimary && count == 1)
                    throw new Exception("Cannot delete the only bank account");

                _context.BankDetails.Remove(bankDetails);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Bank details deleted for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bank details");
                throw;
            }
        }

        public async Task<bool> SetPrimaryBankAsync(string userId, int bankId)
        {
            try
            {
                var bank = await _context.BankDetails
                    .FirstOrDefaultAsync(b => b.Id == bankId && b.UserId == userId);

                if (bank == null)
                    throw new Exception("Bank account not found");

                // Remove primary from other accounts
                var otherPrimaries = await _context.BankDetails
                    .Where(b => b.UserId == userId && b.IsPrimary && b.Id != bankId)
                    .ToListAsync();

                foreach (var other in otherPrimaries)
                {
                    other.IsPrimary = false;
                }

                bank.IsPrimary = true;
                bank.UpdatedAt = DateTime.UtcNow;

                _context.BankDetails.UpdateRange(otherPrimaries);
                _context.BankDetails.Update(bank);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary bank");
                throw;
            }
        }

        private BankDetailsDto MapToDto(BankDetails bank)
        {
            var maskedAccountNumber = bank.AccountNumber.Length > 4
                ? "**** " + bank.AccountNumber.Substring(bank.AccountNumber.Length - 4)
                : "****";

            return new BankDetailsDto
            {
                Id = bank.Id,
                BankName = bank.BankName,
                AccountNumber = bank.AccountNumber,
                MaskedAccountNumber = maskedAccountNumber,
                AccountHolderName = bank.AccountHolderName,
                AccountType = bank.AccountType,
                IBAN = bank.IBAN,
                BranchCode = bank.BranchCode,
                IsVerified = bank.IsVerified,
                IsPrimary = bank.IsPrimary,
                CreatedAt = bank.CreatedAt,
                VerifiedAt = bank.VerifiedAt
            };
        }
    }
}

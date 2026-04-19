using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace NashPay.API.Services
{
    public interface ICommissionService
    {
        Task<CommissionDetailsDto> GetMerchantCommissionAsync(string merchantId);
        Task<MerchantCommissionSummaryDto> GetMerchantCommissionSummaryAsync(string merchantId);
        Task<AdminCommissionConfigDto> GetAdminCommissionConfigAsync(string adminId);
        Task<bool> UpdateMerchantCommissionAsync(string adminId, string merchantId, decimal commissionPercentage);
        Task<CommissionLogDto> LogCommissionAsync(string transactionId, string merchantId, string adminId, decimal amount, decimal commissionPercentage);
        Task<IEnumerable<CommissionLogDto>> GetMerchantCommissionLogsAsync(string merchantId, int limit = 50);
        Task<decimal> CalculateCommissionAsync(decimal amount, decimal commissionPercentage);
    }

    public class CommissionService : ICommissionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CommissionService> _logger;

        public CommissionService(AppDbContext context, ILogger<CommissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CommissionDetailsDto> GetMerchantCommissionAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == merchantId);

                if (merchant == null)
                    throw new Exception("Merchant not found");

                var commissionPercentage = merchant.CommissionPercentage ?? 0m;

                // Calculate totals
                var commissionLogs = await _context.CommissionLogs
                    .Where(c => c.MerchantId == merchantId)
                    .ToListAsync();

                var totalCommission = commissionLogs.Sum(c => c.CommissionAmount);
                var totalPaid = commissionLogs.Where(c => c.Status == "Settled").Sum(c => c.CommissionAmount);
                var pending = commissionLogs.Where(c => c.Status == "Pending").Sum(c => c.CommissionAmount);

                return new CommissionDetailsDto
                {
                    MerchantId = merchantId,
                    MerchantName = merchant.FullName,
                    CommissionPercentage = commissionPercentage,
                    TotalTransactionAmount = commissionLogs.Sum(c => c.TransactionAmount),
                    TotalCommissionEarned = totalCommission,
                    TotalPaid = totalPaid,
                    Pending = pending,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant commission");
                throw;
            }
        }

        public async Task<MerchantCommissionSummaryDto> GetMerchantCommissionSummaryAsync(string merchantId)
        {
            try
            {
                var merchant = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == merchantId);

                if (merchant == null)
                    throw new Exception("Merchant not found");

                var commissionPercentage = merchant.CommissionPercentage ?? 0m;

                // Get recent transactions
                var recentLogs = await _context.CommissionLogs
                    .Where(c => c.MerchantId == merchantId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                var totalEarnings = recentLogs.Sum(c => c.TransactionAmount);
                var totalCommissionDeducted = recentLogs.Sum(c => c.CommissionAmount);
                var netEarnings = totalEarnings - totalCommissionDeducted;

                return new MerchantCommissionSummaryDto
                {
                    CommissionPercentage = commissionPercentage,
                    TotalEarnings = totalEarnings,
                    TotalCommissionDeducted = totalCommissionDeducted,
                    NetEarnings = netEarnings,
                    RecentTransactions = recentLogs.Select(MapCommissionLogToDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant commission summary");
                throw;
            }
        }

        public async Task<AdminCommissionConfigDto> GetAdminCommissionConfigAsync(string adminId)
        {
            try
            {
                var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == adminId && u.Role == "Admin");

                if (admin == null)
                    throw new Exception("Admin not found");

                // Get all merchants and their commission details
                var merchants = await _context.Users
                    .Where(u => u.Role == "Merchant")
                    .ToListAsync();

                var merchantCommissions = new List<CommissionDetailsDto>();

                foreach (var merchant in merchants)
                {
                    var commissionPercentage = merchant.CommissionPercentage ?? 0m;
                    var commissionLogs = await _context.CommissionLogs
                        .Where(c => c.MerchantId == merchant.Id)
                        .ToListAsync();

                    merchantCommissions.Add(new CommissionDetailsDto
                    {
                        MerchantId = merchant.Id,
                        MerchantName = merchant.FullName,
                        CommissionPercentage = commissionPercentage,
                        TotalTransactionAmount = commissionLogs.Sum(c => c.TransactionAmount),
                        TotalCommissionEarned = commissionLogs.Sum(c => c.CommissionAmount),
                        TotalPaid = commissionLogs.Where(c => c.Status == "Settled").Sum(c => c.CommissionAmount),
                        Pending = commissionLogs.Where(c => c.Status == "Pending").Sum(c => c.CommissionAmount),
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                return new AdminCommissionConfigDto
                {
                    AdminId = adminId,
                    DefaultCommissionPercentage = 0m,
                    MerchantCommissions = merchantCommissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin commission config");
                throw;
            }
        }

        public async Task<bool> UpdateMerchantCommissionAsync(string adminId, string merchantId, decimal commissionPercentage)
        {
            try
            {
                var admin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == adminId && u.Role == "Admin");

                if (admin == null)
                    throw new Exception("Admin not found");

                var merchant = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == merchantId && u.Role == "Merchant");

                if (merchant == null)
                    throw new Exception("Merchant not found");

                if (commissionPercentage < 0 || commissionPercentage > 100)
                    throw new Exception("Commission percentage must be between 0 and 100");

                merchant.CommissionPercentage = commissionPercentage;
                merchant.UpdatedAt = DateTime.UtcNow;

                _context.Users.Update(merchant);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Commission updated for merchant {merchantId} by admin {adminId}: {commissionPercentage}%");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating merchant commission");
                throw;
            }
        }

        public async Task<CommissionLogDto> LogCommissionAsync(string transactionId, string merchantId, string adminId, decimal amount, decimal commissionPercentage)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaction == null)
                    throw new Exception("Transaction not found");

                var commissionAmount = amount * (commissionPercentage / 100);
                var merchantAmount = amount - commissionAmount;

                var commissionLog = new CommissionLog
                {
                    TransactionId = transactionId,
                    MerchantId = merchantId,
                    AdminId = adminId,
                    TransactionAmount = amount,
                    CommissionPercentage = commissionPercentage,
                    CommissionAmount = commissionAmount,
                    MerchantAmount = merchantAmount,
                    AdminAmount = commissionAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.CommissionLogs.Add(commissionLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Commission logged for transaction {transactionId}: {commissionPercentage}% = ₨{commissionAmount}");
                return MapCommissionLogToDto(commissionLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging commission");
                throw;
            }
        }

        public async Task<IEnumerable<CommissionLogDto>> GetMerchantCommissionLogsAsync(string merchantId, int limit = 50)
        {
            var logs = await _context.CommissionLogs
                .Where(c => c.MerchantId == merchantId)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return logs.Select(MapCommissionLogToDto);
        }

        public async Task<decimal> CalculateCommissionAsync(decimal amount, decimal commissionPercentage)
        {
            return amount * (commissionPercentage / 100);
        }

        private CommissionLogDto MapCommissionLogToDto(CommissionLog log)
        {
            return new CommissionLogDto
            {
                Id = log.Id,
                TransactionId = log.TransactionId,
                TransactionAmount = log.TransactionAmount,
                CommissionPercentage = log.CommissionPercentage,
                CommissionAmount = log.CommissionAmount,
                MerchantAmount = log.MerchantAmount,
                AdminAmount = log.AdminAmount,
                Status = log.Status,
                CreatedAt = log.CreatedAt,
                SettledAt = log.SettledAt
            };
        }
    }
}

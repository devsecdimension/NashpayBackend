using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace NashPay.API.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
        Task<UserDto> GetUserDetailsAsync(string userId);
        Task<bool> UpdateUserStatusAsync(string userId, bool isActive);
        Task<bool> ApproveKYCAsync(string userId);
        Task<bool> RejectKYCAsync(string userId, string reason);
        Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync(int pageNumber = 1, int pageSize = 50);
        Task<IEnumerable<FraudAlertDto>> GetFraudAlertsAsync();
        Task<bool> ResolveFraudAlertAsync(int alertId, string resolution);
    }

    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            AppDbContext context,
            UserManager<User> userManager,
            ILogger<AdminService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(MapToUserDto);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            var users = await _context.Users
                .Where(u => u.Role == role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(MapToUserDto);
        }

        public async Task<UserDto> GetUserDetailsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            return MapToUserDto(user);
        }

        public async Task<bool> UpdateUserStatusAsync(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<bool> ApproveKYCAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                user.KYCStatus = "Approved";
                user.IsVerified = true;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"KYC approved for user {userId}");
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KYC approval error");
                throw;
            }
        }

        public async Task<bool> RejectKYCAsync(string userId, string reason)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                user.KYCStatus = "Rejected";

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"KYC rejected for user {userId}: {reason}");
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KYC rejection error");
                throw;
            }
        }

        public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync(int pageNumber = 1, int pageSize = 50)
        {
            var transactions = await _context.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return transactions.Select(MapToTransactionDto);
        }

        public async Task<IEnumerable<FraudAlertDto>> GetFraudAlertsAsync()
        {
            var alerts = await _context.FraudAlerts
                .Where(a => a.Status == "Open" || a.Status == "Investigating")
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return alerts.Select(MapToFraudAlertDto);
        }

        public async Task<bool> ResolveFraudAlertAsync(int alertId, string resolution)
        {
            try
            {
                var alert = await _context.FraudAlerts
                    .FirstOrDefaultAsync(a => a.Id == alertId);

                if (alert == null)
                    throw new Exception("Fraud alert not found");

                alert.Status = "Resolved";
                alert.Resolution = resolution;
                alert.ResolvedAt = DateTime.UtcNow;

                _context.FraudAlerts.Update(alert);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Fraud alert {alertId} resolved");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fraud alert resolution error");
                throw;
            }
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                KYCStatus = user.KYCStatus,
                BusinessName = user.BusinessName,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            };
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

        private FraudAlertDto MapToFraudAlertDto(FraudAlert alert)
        {
            return new FraudAlertDto
            {
                Id = alert.Id,
                UserId = alert.UserId,
                AlertType = alert.AlertType,
                Description = alert.Description,
                SuspiciousAmount = alert.SuspiciousAmount,
                Status = alert.Status,
                Resolution = alert.Resolution,
                CreatedAt = alert.CreatedAt,
                ResolvedAt = alert.ResolvedAt
            };
        }
    }
}

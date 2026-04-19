using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;

namespace NashPay.API.Services
{
    public interface ICheckoutService
    {
        Task<CheckoutResponseDto> InitiateCheckoutAsync(string merchantId, InitiateCheckoutDto model);
        Task<CheckoutSessionDto> GetCheckoutSessionAsync(string checkoutSessionId);
        Task<bool> CompleteCheckoutAsync(string checkoutSessionId, string transactionId);
        Task<bool> CancelCheckoutAsync(string checkoutSessionId);
        Task<bool> ExpireOldCheckoutsAsync();
        Task<IEnumerable<CheckoutSessionDto>> GetMerchantCheckoutsAsync(string merchantId, int pageNumber = 1, int pageSize = 20);
    }

    public class CheckoutService : ICheckoutService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CheckoutService> _logger;
        private readonly IPaymentService _paymentService;

        public CheckoutService(AppDbContext context, ILogger<CheckoutService> logger, IPaymentService paymentService)
        {
            _context = context;
            _logger = logger;
            _paymentService = paymentService;
        }

        public async Task<CheckoutResponseDto> InitiateCheckoutAsync(string merchantId, InitiateCheckoutDto model)
        {
            try
            {
                // Validate merchant exists
                var merchant = await _context.Users.FirstOrDefaultAsync(u => u.Id == merchantId);
                if (merchant == null)
                    throw new Exception("Merchant not found");

                // Generate unique checkout session ID
                string checkoutSessionId = $"CHKOUT_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                // Create checkout session
                var session = new CheckoutSession
                {
                    CheckoutSessionId = checkoutSessionId,
                    MerchantId = merchantId,
                    OrderId = model.OrderId,
                    Amount = model.Amount,
                    Currency = "PKR",
                    ProductName = model.ProductName,
                    ProductDescription = model.ProductDescription,
                    ReturnUrl = model.ReturnUrl,
                    CancelUrl = model.CancelUrl,
                    CustomerEmail = model.CustomerEmail,
                    CustomerName = model.CustomerName,
                    Status = "pending",
                    Metadata = System.Text.Json.JsonSerializer.Serialize(model.Metadata),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };

                _context.CheckoutSessions.Add(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checkout session created: {checkoutSessionId} for merchant {merchantId}");

                return new CheckoutResponseDto
                {
                    CheckoutSessionId = checkoutSessionId,
                    CheckoutUrl = $"https://localhost:5001/checkout/{checkoutSessionId}", // Frontend URL
                    OrderId = model.OrderId,
                    Amount = model.Amount,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = session.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout initiation failed");
                throw new Exception($"Checkout initiation failed: {ex.Message}");
            }
        }

        public async Task<CheckoutSessionDto> GetCheckoutSessionAsync(string checkoutSessionId)
        {
            try
            {
                var session = await _context.CheckoutSessions
                    .FirstOrDefaultAsync(s => s.CheckoutSessionId == checkoutSessionId);

                if (session == null)
                    throw new KeyNotFoundException("Checkout session not found");

                // Check if expired
                if (session.ExpiresAt < DateTime.UtcNow && session.Status == "pending")
                {
                    session.Status = "expired";
                    await _context.SaveChangesAsync();
                }

                var metadata = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(session.Metadata))
                {
                    try
                    {
                        metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(session.Metadata);
                    }
                    catch { }
                }

                return new CheckoutSessionDto
                {
                    CheckoutSessionId = session.CheckoutSessionId,
                    MerchantId = session.MerchantId,
                    OrderId = session.OrderId,
                    Amount = session.Amount,
                    ProductName = session.ProductName,
                    Status = session.Status,
                    TransactionId = session.TransactionId,
                    CreatedAt = session.CreatedAt,
                    CompletedAt = session.CompletedAt,
                    Metadata = metadata
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get checkout session error");
                throw;
            }
        }

        public async Task<bool> CompleteCheckoutAsync(string checkoutSessionId, string transactionId)
        {
            try
            {
                var session = await _context.CheckoutSessions
                    .FirstOrDefaultAsync(s => s.CheckoutSessionId == checkoutSessionId);

                if (session == null)
                    throw new KeyNotFoundException("Checkout session not found");

                session.Status = "completed";
                session.TransactionId = transactionId;
                session.CompletedAt = DateTime.UtcNow;

                _context.CheckoutSessions.Update(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checkout completed: {checkoutSessionId}, Transaction: {transactionId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Complete checkout error");
                throw;
            }
        }

        public async Task<bool> CancelCheckoutAsync(string checkoutSessionId)
        {
            try
            {
                var session = await _context.CheckoutSessions
                    .FirstOrDefaultAsync(s => s.CheckoutSessionId == checkoutSessionId);

                if (session == null)
                    throw new KeyNotFoundException("Checkout session not found");

                session.Status = "cancelled";
                session.CancelledAt = DateTime.UtcNow;

                _context.CheckoutSessions.Update(session);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Checkout cancelled: {checkoutSessionId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cancel checkout error");
                throw;
            }
        }

        public async Task<bool> ExpireOldCheckoutsAsync()
        {
            try
            {
                var expiredSessions = await _context.CheckoutSessions
                    .Where(s => s.Status == "pending" && s.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var session in expiredSessions)
                {
                    session.Status = "expired";
                }

                if (expiredSessions.Count > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Expired {expiredSessions.Count} old checkout sessions");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Expire checkout sessions error");
                return false;
            }
        }

        public async Task<IEnumerable<CheckoutSessionDto>> GetMerchantCheckoutsAsync(string merchantId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var sessions = await _context.CheckoutSessions
                    .Where(s => s.MerchantId == merchantId)
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return sessions.Select(s => new CheckoutSessionDto
                {
                    CheckoutSessionId = s.CheckoutSessionId,
                    MerchantId = s.MerchantId,
                    OrderId = s.OrderId,
                    Amount = s.Amount,
                    ProductName = s.ProductName,
                    Status = s.Status,
                    TransactionId = s.TransactionId,
                    CreatedAt = s.CreatedAt,
                    CompletedAt = s.CompletedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get merchant checkouts error");
                throw;
            }
        }
    }
}

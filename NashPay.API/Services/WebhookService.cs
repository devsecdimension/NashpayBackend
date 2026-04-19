using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NashPay.API.Services
{
    public interface IWebhookService
    {
        // Webhook Endpoint Management
        Task<WebhookDetailDto> RegisterWebhookAsync(string userId, RegisterWebhookDto model);
        Task<IEnumerable<WebhookDetailDto>> GetUserWebhooksAsync(string userId);
        Task<WebhookDetailDto> GetWebhookAsync(string webhookId);
        Task<bool> UpdateWebhookAsync(string webhookId, UpdateWebhookDto model);
        Task<bool> DeleteWebhookAsync(string webhookId);
        Task<bool> ToggleWebhookAsync(string webhookId, bool isActive);

        // Webhook Delivery
        Task<bool> SendWebhookAsync(string eventType, Dictionary<string, object> data, string userId = null);
        Task<bool> RetryFailedWebhooksAsync();
        Task<IEnumerable<WebhookLogDetailDto>> GetWebhookLogsAsync(string webhookId, int pageNumber = 1, int pageSize = 20);

        // Webhook Verification
        string GenerateWebhookSignature(string payload, string secret);
        bool VerifyWebhookSignature(string payload, string signature, string secret);
    }

    public class WebhookService : IWebhookService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WebhookService> _logger;
        private readonly HttpClient _httpClient;

        public WebhookService(AppDbContext context, ILogger<WebhookService> logger, HttpClient httpClient)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
        }

        // ===== Webhook Endpoint Management =====

        public async Task<WebhookDetailDto> RegisterWebhookAsync(string userId, RegisterWebhookDto model)
        {
            try
            {
                // Validate user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new Exception("User not found");

                // Generate webhook ID and secret
                string webhookId = $"WHK_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                string secret = GenerateRandomSecret(32);

                var webhook = new WebhookEndpoint
                {
                    WebhookId = webhookId,
                    UserId = userId,
                    Url = model.Url,
                    Events = string.Join(",", model.Events),
                    Description = model.Description,
                    IsActive = model.IsActive,
                    Secret = secret,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WebhookEndpoints.Add(webhook);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Webhook registered: {webhookId} for user {userId}");

                return new WebhookDetailDto
                {
                    WebhookId = webhookId,
                    Url = model.Url,
                    Events = model.Events,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    SuccessfulDeliveries = 0,
                    FailedDeliveries = 0,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register webhook error");
                throw new Exception($"Webhook registration failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<WebhookDetailDto>> GetUserWebhooksAsync(string userId)
        {
            try
            {
                var webhooks = await _context.WebhookEndpoints
                    .Where(w => w.UserId == userId)
                    .ToListAsync();

                return webhooks.Select(w => new WebhookDetailDto
                {
                    WebhookId = w.WebhookId,
                    Url = w.Url,
                    Events = w.Events.Split(",").ToList(),
                    Description = w.Description,
                    IsActive = w.IsActive,
                    SuccessfulDeliveries = w.SuccessfulDeliveries,
                    FailedDeliveries = w.FailedDeliveries,
                    LastDeliveryAt = w.LastDeliveryAt,
                    CreatedAt = w.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user webhooks error");
                throw;
            }
        }

        public async Task<WebhookDetailDto> GetWebhookAsync(string webhookId)
        {
            try
            {
                var webhook = await _context.WebhookEndpoints
                    .FirstOrDefaultAsync(w => w.WebhookId == webhookId);

                if (webhook == null)
                    throw new KeyNotFoundException("Webhook not found");

                return new WebhookDetailDto
                {
                    WebhookId = webhookId,
                    Url = webhook.Url,
                    Events = webhook.Events.Split(",").ToList(),
                    Description = webhook.Description,
                    IsActive = webhook.IsActive,
                    SuccessfulDeliveries = webhook.SuccessfulDeliveries,
                    FailedDeliveries = webhook.FailedDeliveries,
                    LastDeliveryAt = webhook.LastDeliveryAt,
                    CreatedAt = webhook.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get webhook error");
                throw;
            }
        }

        public async Task<bool> UpdateWebhookAsync(string webhookId, UpdateWebhookDto model)
        {
            try
            {
                var webhook = await _context.WebhookEndpoints
                    .FirstOrDefaultAsync(w => w.WebhookId == webhookId);

                if (webhook == null)
                    throw new KeyNotFoundException("Webhook not found");

                webhook.Url = model.Url;
                webhook.Events = string.Join(",", model.Events);
                webhook.Description = model.Description;
                webhook.IsActive = model.IsActive;
                webhook.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Webhook updated: {webhookId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update webhook error");
                throw;
            }
        }

        public async Task<bool> DeleteWebhookAsync(string webhookId)
        {
            try
            {
                var webhook = await _context.WebhookEndpoints
                    .FirstOrDefaultAsync(w => w.WebhookId == webhookId);

                if (webhook == null)
                    throw new KeyNotFoundException("Webhook not found");

                _context.WebhookEndpoints.Remove(webhook);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Webhook deleted: {webhookId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete webhook error");
                throw;
            }
        }

        public async Task<bool> ToggleWebhookAsync(string webhookId, bool isActive)
        {
            try
            {
                var webhook = await _context.WebhookEndpoints
                    .FirstOrDefaultAsync(w => w.WebhookId == webhookId);

                if (webhook == null)
                    throw new KeyNotFoundException("Webhook not found");

                webhook.IsActive = isActive;
                webhook.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Webhook toggled: {webhookId}, Active: {isActive}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toggle webhook error");
                throw;
            }
        }

        // ===== Webhook Delivery =====

        public async Task<bool> SendWebhookAsync(string eventType, Dictionary<string, object> data, string userId = null)
        {
            try
            {
                // Get all active webhooks that subscribe to this event
                var webhooks = await _context.WebhookEndpoints
                    .Where(w => w.IsActive && (userId == null || w.UserId == userId))
                    .ToListAsync();

                var targetWebhooks = webhooks.Where(w => w.Events.Contains(eventType)).ToList();

                if (targetWebhooks.Count == 0)
                {
                    _logger.LogInformation($"No webhooks found for event: {eventType}");
                    return true;
                }

                var eventId = $"EVT_{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                var payload = new WebhookPayloadDto
                {
                    EventId = eventId,
                    EventType = eventType,
                    Timestamp = DateTime.UtcNow,
                    Data = data
                };

                // Send to each webhook
                foreach (var webhook in targetWebhooks)
                {
                    _ = Task.Run(async () => await DeliverWebhookAsync(webhook, payload));
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send webhook error");
                return false;
            }
        }

        private async Task DeliverWebhookAsync(WebhookEndpoint webhook, WebhookPayloadDto payload)
        {
            try
            {
                var payloadJson = JsonSerializer.Serialize(payload);
                var signature = GenerateWebhookSignature(payloadJson, webhook.Secret);

                var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
                content.Headers.Add("X-Webhook-Signature", signature);
                content.Headers.Add("X-Webhook-ID", payload.EventId);
                content.Headers.Add("X-Webhook-Timestamp", payload.Timestamp.ToString("O"));

                var response = await _httpClient.PostAsync(webhook.Url, content);

                // Log the delivery
                var logEntry = new WebhookLog
                {
                    UserId = webhook.UserId,
                    WebhookUrl = webhook.Url,
                    EventType = payload.EventType,
                    Payload = payloadJson,
                    HttpStatusCode = (int)response.StatusCode,
                    ResponseBody = await response.Content.ReadAsStringAsync(),
                    IsSuccess = response.IsSuccessStatusCode,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                if (response.IsSuccessStatusCode)
                {
                    webhook.SuccessfulDeliveries++;
                    webhook.LastDeliveryAt = DateTime.UtcNow;
                    _logger.LogInformation($"Webhook delivered successfully: {webhook.WebhookId}");
                }
                else
                {
                    webhook.FailedDeliveries++;
                    webhook.LastFailureAt = DateTime.UtcNow;
                    logEntry.ErrorMessage = $"HTTP {response.StatusCode}";
                    _logger.LogWarning($"Webhook delivery failed: {webhook.WebhookId}, Status: {response.StatusCode}");
                }

                _context.WebhookLogs.Add(logEntry);
                _context.WebhookEndpoints.Update(webhook);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Webhook delivery error for {webhook.WebhookId}");

                var logEntry = new WebhookLog
                {
                    UserId = webhook.UserId,
                    WebhookUrl = webhook.Url,
                    EventType = payload.EventType,
                    Payload = JsonSerializer.Serialize(payload),
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                webhook.FailedDeliveries++;
                webhook.LastFailureAt = DateTime.UtcNow;

                _context.WebhookLogs.Add(logEntry);
                _context.WebhookEndpoints.Update(webhook);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> RetryFailedWebhooksAsync()
        {
            try
            {
                // Get failed webhook logs with retry count < 3
                var failedLogs = await _context.WebhookLogs
                    .Where(l => !l.IsSuccess && l.RetryCount < 3)
                    .OrderBy(l => l.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                foreach (var log in failedLogs)
                {
                    try
                    {
                        var webhook = await _context.WebhookEndpoints
                            .FirstOrDefaultAsync(w => w.Url == log.WebhookUrl);

                        if (webhook == null) continue;

                        var payload = JsonSerializer.Deserialize<WebhookPayloadDto>(log.Payload);
                        
                        // Retry delivery
                        await DeliverWebhookAsync(webhook, payload);

                        log.RetryCount++;
                        _context.WebhookLogs.Update(log);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Retry webhook error");
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Retry failed webhooks error");
                return false;
            }
        }

        public async Task<IEnumerable<WebhookLogDetailDto>> GetWebhookLogsAsync(string webhookId, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var webhook = await _context.WebhookEndpoints
                    .FirstOrDefaultAsync(w => w.WebhookId == webhookId);

                if (webhook == null)
                    throw new KeyNotFoundException("Webhook not found");

                var logs = await _context.WebhookLogs
                    .Where(l => l.WebhookUrl == webhook.Url)
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return logs.Select(l => new WebhookLogDetailDto
                {
                    LogId = l.Id.ToString(),
                    WebhookUrl = l.WebhookUrl,
                    EventType = l.EventType,
                    Payload = l.Payload,
                    HttpStatusCode = l.HttpStatusCode,
                    ResponseBody = l.ResponseBody,
                    IsSuccess = l.IsSuccess,
                    ErrorMessage = l.ErrorMessage,
                    RetryCount = l.RetryCount,
                    CreatedAt = l.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get webhook logs error");
                throw;
            }
        }

        // ===== Webhook Security =====

        public string GenerateWebhookSignature(string payload, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return $"sha256={Convert.ToHexString(hash).ToLower()}";
            }
        }

        public bool VerifyWebhookSignature(string payload, string signature, string secret)
        {
            try
            {
                var expectedSignature = GenerateWebhookSignature(payload, secret);
                return signature == expectedSignature;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook signature verification error");
                return false;
            }
        }

        private string GenerateRandomSecret(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenData = new byte[length];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }
    }
}

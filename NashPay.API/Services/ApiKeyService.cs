using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using NashPay.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace NashPay.API.Services
{
    public interface IApiKeyService
    {
        Task<ApiKeyResponseDto> CreateApiKeyAsync(string userId, CreateApiKeyDto model);
        Task<IEnumerable<ApiKeyDto>> GetUserApiKeysAsync(string userId);
        Task<bool> RevokeApiKeyAsync(string userId, int keyId);
        Task<bool> UpdateLastUsedAsync(string publicKey);
        Task<ApiKeyDto> ValidateApiKeyAsync(string publicKey, string secretKey);
    }

    public class ApiKeyService : IApiKeyService
    {
        private readonly AppDbContext _context;
        private readonly EncryptionHelper _encryptionHelper;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(
            AppDbContext context,
            EncryptionHelper encryptionHelper,
            ILogger<ApiKeyService> logger)
        {
            _context = context;
            _encryptionHelper = encryptionHelper;
            _logger = logger;
        }

        public async Task<ApiKeyResponseDto> CreateApiKeyAsync(string userId, CreateApiKeyDto model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new Exception("User not found");

                // Generate keys
                string publicKey = $"pk_live_{EncryptionHelper.GenerateRandomKey(16)}";
                string secretKey = $"sk_live_{EncryptionHelper.GenerateRandomKey(24)}";

                // Encrypt secret key before storing
                string encryptedSecret = _encryptionHelper.Encrypt(secretKey);

                var apiKey = new ApiKey
                {
                    UserId = userId,
                    PublicKey = publicKey,
                    SecretKey = encryptedSecret,
                    KeyName = model.KeyName,
                    Environment = model.Environment,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    RequestsPerMinute = 100
                };

                _context.ApiKeys.Add(apiKey);
                await _context.SaveChangesAsync();

                return new ApiKeyResponseDto
                {
                    PublicKey = publicKey,
                    SecretKey = secretKey, // Only shown once at creation
                    Message = "API Key created successfully. Save your secret key securely!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create API key error");
                throw new Exception($"API key creation failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ApiKeyDto>> GetUserApiKeysAsync(string userId)
        {
            var keys = await _context.ApiKeys
                .Where(k => k.UserId == userId && k.RevokedAt == null)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();

            return keys.Select(k => new ApiKeyDto
            {
                Id = k.Id,
                PublicKey = k.PublicKey,
                SecretKey = "*****", // Never expose secret key
                KeyName = k.KeyName,
                Environment = k.Environment,
                IsActive = k.IsActive,
                LastUsedAt = k.LastUsedAt,
                CreatedAt = k.CreatedAt
            });
        }

        public async Task<bool> RevokeApiKeyAsync(string userId, int keyId)
        {
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Id == keyId && k.UserId == userId);

            if (apiKey == null)
                throw new Exception("API Key not found");

            apiKey.IsActive = false;
            apiKey.RevokedAt = DateTime.UtcNow;

            _context.ApiKeys.Update(apiKey);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLastUsedAsync(string publicKey)
        {
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.PublicKey == publicKey && k.IsActive);

            if (apiKey != null)
            {
                apiKey.LastUsedAt = DateTime.UtcNow;
                _context.ApiKeys.Update(apiKey);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<ApiKeyDto> ValidateApiKeyAsync(string publicKey, string secretKey)
        {
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.PublicKey == publicKey && k.IsActive);

            if (apiKey == null)
                throw new Exception("API Key not found");

            try
            {
                string decryptedSecret = _encryptionHelper.Decrypt(apiKey.SecretKey);
                if (decryptedSecret != secretKey)
                    throw new Exception("Invalid secret key");

                apiKey.LastUsedAt = DateTime.UtcNow;
                _context.ApiKeys.Update(apiKey);
                await _context.SaveChangesAsync();

                return new ApiKeyDto
                {
                    Id = apiKey.Id,
                    PublicKey = apiKey.PublicKey,
                    SecretKey = "*****",
                    KeyName = apiKey.KeyName,
                    Environment = apiKey.Environment,
                    IsActive = apiKey.IsActive,
                    LastUsedAt = apiKey.LastUsedAt,
                    CreatedAt = apiKey.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API key validation error");
                throw new Exception("API Key validation failed");
            }
        }
    }
}

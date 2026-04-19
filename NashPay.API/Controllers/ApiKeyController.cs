using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NashPay.API.DTOs;
using NashPay.API.Services;

namespace NashPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiKeyController> _logger;

        public ApiKeyController(IApiKeyService apiKeyService, ILogger<ApiKeyController> logger)
        {
            _apiKeyService = apiKeyService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponseDto<ApiKeyResponseDto>>> CreateApiKey([FromBody] CreateApiKeyDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _apiKeyService.CreateApiKeyAsync(userId, model);
                return Ok(new ApiResponseDto<ApiKeyResponseDto>(true, "API Key created successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create API key error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("list")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<ApiKeyDto>>>> GetApiKeys()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _apiKeyService.GetUserApiKeysAsync(userId);
                return Ok(new ApiResponseDto<IEnumerable<ApiKeyDto>>(true, "API Keys retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get API keys error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{keyId}/revoke")]
        public async Task<ActionResult<ApiResponseDto>> RevokeApiKey(int keyId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _apiKeyService.RevokeApiKeyAsync(userId, keyId);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Revocation failed"));

                return Ok(new ApiResponseDto(true, "API Key revoked successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Revoke API key error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<ApiKeyDto>>> ValidateApiKey([FromBody] dynamic credentials)
        {
            try
            {
                string publicKey = credentials.publicKey;
                string secretKey = credentials.secretKey;

                if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(secretKey))
                    return BadRequest(new ApiResponseDto(false, "Invalid credentials"));

                var result = await _apiKeyService.ValidateApiKeyAsync(publicKey, secretKey);
                return Ok(new ApiResponseDto<ApiKeyDto>(true, "API Key validated successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validate API key error");
                return Unauthorized(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

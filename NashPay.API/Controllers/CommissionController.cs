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
    public class CommissionController : ControllerBase
    {
        private readonly ICommissionService _commissionService;
        private readonly ILogger<CommissionController> _logger;

        public CommissionController(ICommissionService commissionService, ILogger<CommissionController> logger)
        {
            _commissionService = commissionService;
            _logger = logger;
        }

        // Merchant endpoints
        [HttpGet("my-commission")]
        public async Task<ActionResult<ApiResponseDto<CommissionDetailsDto>>> GetMyCommission()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _commissionService.GetMerchantCommissionAsync(userId);
                return Ok(new ApiResponseDto<CommissionDetailsDto>(true, "Commission details retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get merchant commission error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("my-summary")]
        public async Task<ActionResult<ApiResponseDto<MerchantCommissionSummaryDto>>> GetMyCommissionSummary()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _commissionService.GetMerchantCommissionSummaryAsync(userId);
                return Ok(new ApiResponseDto<MerchantCommissionSummaryDto>(true, "Commission summary retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get commission summary error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("my-logs")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CommissionLogDto>>>> GetMyCommissionLogs([FromQuery] int limit = 50)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _commissionService.GetMerchantCommissionLogsAsync(userId, limit);
                return Ok(new ApiResponseDto<IEnumerable<CommissionLogDto>>(true, "Commission logs retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get commission logs error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        // Admin endpoints
        [HttpGet("admin/config")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<AdminCommissionConfigDto>>> GetAdminConfig()
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new ApiResponseDto(false, "Admin not found"));

                var result = await _commissionService.GetAdminCommissionConfigAsync(adminId);
                return Ok(new ApiResponseDto<AdminCommissionConfigDto>(true, "Admin commission config retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get admin config error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("admin/merchant/{merchantId}/set-commission")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto>> SetMerchantCommission(string merchantId, [FromBody] SetCommissionDto model)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new ApiResponseDto(false, "Admin not found"));

                var result = await _commissionService.UpdateMerchantCommissionAsync(adminId, merchantId, model.CommissionPercentage);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Commission update failed"));

                return Ok(new ApiResponseDto(true, "Merchant commission updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set merchant commission error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("admin/merchant/{merchantId}/logs")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CommissionLogDto>>>> GetMerchantLogs(string merchantId, [FromQuery] int limit = 50)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                    return Unauthorized(new ApiResponseDto(false, "Admin not found"));

                var result = await _commissionService.GetMerchantCommissionLogsAsync(merchantId, limit);
                return Ok(new ApiResponseDto<IEnumerable<CommissionLogDto>>(true, "Merchant commission logs retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get merchant logs error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("calculate")]
        public async Task<ActionResult<ApiResponseDto<decimal>>> CalculateCommission([FromBody] CalculateCommissionDto model)
        {
            try
            {
                var result = await _commissionService.CalculateCommissionAsync(model.Amount, model.CommissionPercentage);
                return Ok(new ApiResponseDto<decimal>(true, "Commission calculated successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Calculate commission error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

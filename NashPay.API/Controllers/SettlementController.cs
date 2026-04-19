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
    public class SettlementController : ControllerBase
    {
        private readonly ISettlementService _settlementService;
        private readonly ILogger<SettlementController> _logger;

        public SettlementController(ISettlementService settlementService, ILogger<SettlementController> logger)
        {
            _settlementService = settlementService;
            _logger = logger;
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponseDto<SettlementResponseDto>>> InitiateSettlement([FromBody] InitiateSettlementDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _settlementService.InitiateSettlementAsync(userId, model);
                return Ok(new ApiResponseDto<SettlementResponseDto>(true, "Settlement initiated successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initiate settlement error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("{settlementId}")]
        public async Task<ActionResult<ApiResponseDto<SettlementDto>>> GetSettlement(string settlementId)
        {
            try
            {
                var result = await _settlementService.GetSettlementAsync(settlementId);
                return Ok(new ApiResponseDto<SettlementDto>(true, "Settlement retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get settlement error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("user/settlements")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<SettlementDto>>>> GetUserSettlements()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _settlementService.GetUserSettlementsAsync(userId);
                return Ok(new ApiResponseDto<IEnumerable<SettlementDto>>(true, "Settlements retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get settlements error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{settlementId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto>> UpdateSettlementStatus(string settlementId, [FromBody] string status)
        {
            try
            {
                var result = await _settlementService.UpdateSettlementStatusAsync(settlementId, status);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Settlement update failed"));

                return Ok(new ApiResponseDto(true, "Settlement status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update settlement status error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{userId}/simulate-t3")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto>> SimulateT3Settlement(string userId, [FromBody] decimal amount)
        {
            try
            {
                var result = await _settlementService.SimulateT3SettlementAsync(userId, amount);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Simulation failed"));

                return Ok(new ApiResponseDto(true, "T+3 settlement simulated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "T+3 simulation error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

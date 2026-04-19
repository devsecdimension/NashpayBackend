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
    public class BankDetailsController : ControllerBase
    {
        private readonly IBankDetailsService _bankDetailsService;
        private readonly ILogger<BankDetailsController> _logger;

        public BankDetailsController(IBankDetailsService bankDetailsService, ILogger<BankDetailsController> logger)
        {
            _bankDetailsService = bankDetailsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<BankDetailsDto>>>> GetBankDetails()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _bankDetailsService.GetBankDetailsAsync(userId);
                return Ok(new ApiResponseDto<IEnumerable<BankDetailsDto>>(true, "Bank details retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get bank details error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("primary")]
        public async Task<ActionResult<ApiResponseDto<BankDetailsDto>>> GetPrimaryBankDetails()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _bankDetailsService.GetPrimaryBankDetailsAsync(userId);
                return Ok(new ApiResponseDto<BankDetailsDto>(true, "Primary bank details retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get primary bank details error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<ApiResponseDto<BankDetailsDto>>> AddBankDetails([FromBody] CreateBankDetailsDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _bankDetailsService.AddBankDetailsAsync(userId, model);
                return Ok(new ApiResponseDto<BankDetailsDto>(true, "Bank details added successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Add bank details error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPut("{bankId}")]
        public async Task<ActionResult<ApiResponseDto<BankDetailsDto>>> UpdateBankDetails(int bankId, [FromBody] CreateBankDetailsDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _bankDetailsService.UpdateBankDetailsAsync(userId, bankId, model);
                return Ok(new ApiResponseDto<BankDetailsDto>(true, "Bank details updated successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update bank details error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpDelete("{bankId}")]
        public async Task<ActionResult<ApiResponseDto>> DeleteBankDetails(int bankId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _bankDetailsService.DeleteBankDetailsAsync(userId, bankId);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Delete failed"));

                return Ok(new ApiResponseDto(true, "Bank details deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete bank details error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{bankId}/set-primary")]
        public async Task<ActionResult<ApiResponseDto>> SetPrimaryBank(int bankId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _bankDetailsService.SetPrimaryBankAsync(userId, bankId);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Set primary failed"));

                return Ok(new ApiResponseDto(true, "Primary bank set successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set primary bank error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

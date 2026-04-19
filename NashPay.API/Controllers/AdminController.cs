using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NashPay.API.DTOs;
using NashPay.API.Services;

namespace NashPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<UserDto>>>> GetAllUsers()
        {
            try
            {
                var result = await _adminService.GetAllUsersAsync();
                return Ok(new ApiResponseDto<IEnumerable<UserDto>>(true, "Users retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get all users error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("users/role/{role}")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<UserDto>>>> GetUsersByRole(string role)
        {
            try
            {
                var result = await _adminService.GetUsersByRoleAsync(role);
                return Ok(new ApiResponseDto<IEnumerable<UserDto>>(true, $"{role}s retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get users by role error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUserDetails(string userId)
        {
            try
            {
                var result = await _adminService.GetUserDetailsAsync(userId);
                return Ok(new ApiResponseDto<UserDto>(true, "User details retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user details error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPut("users/{userId}/status")]
        public async Task<ActionResult<ApiResponseDto>> UpdateUserStatus(string userId, [FromBody] bool isActive)
        {
            try
            {
                var result = await _adminService.UpdateUserStatusAsync(userId, isActive);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "User status update failed"));

                return Ok(new ApiResponseDto(true, "User status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update user status error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("kyc/{userId}/approve")]
        public async Task<ActionResult<ApiResponseDto>> ApproveKYC(string userId)
        {
            try
            {
                var result = await _adminService.ApproveKYCAsync(userId);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "KYC approval failed"));

                return Ok(new ApiResponseDto(true, "KYC approved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Approve KYC error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("kyc/{userId}/reject")]
        public async Task<ActionResult<ApiResponseDto>> RejectKYC(string userId, [FromBody] string reason)
        {
            try
            {
                var result = await _adminService.RejectKYCAsync(userId, reason);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "KYC rejection failed"));

                return Ok(new ApiResponseDto(true, "KYC rejected successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reject KYC error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<TransactionDto>>>> GetAllTransactions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _adminService.GetAllTransactionsAsync(pageNumber, pageSize);
                return Ok(new ApiResponseDto<IEnumerable<TransactionDto>>(true, "Transactions retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get transactions error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("fraud-alerts")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<FraudAlertDto>>>> GetFraudAlerts()
        {
            try
            {
                var result = await _adminService.GetFraudAlertsAsync();
                return Ok(new ApiResponseDto<IEnumerable<FraudAlertDto>>(true, "Fraud alerts retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get fraud alerts error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("fraud-alerts/{alertId}/resolve")]
        public async Task<ActionResult<ApiResponseDto>> ResolveFraudAlert(int alertId, [FromBody] string resolution)
        {
            try
            {
                var result = await _adminService.ResolveFraudAlertAsync(alertId, resolution);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Alert resolution failed"));

                return Ok(new ApiResponseDto(true, "Fraud alert resolved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resolve fraud alert error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

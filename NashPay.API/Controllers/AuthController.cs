using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NashPay.API.DTOs;
using NashPay.API.Services;

namespace NashPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Register([FromBody] RegisterDto model)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponseDto(false, "Invalid input", GetModelErrors()));

                var result = await _authService.RegisterAsync(model);
                return Ok(new ApiResponseDto<LoginResponseDto>(true, "Registration successful", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponseDto(false, "Invalid input", GetModelErrors()));

                var result = await _authService.LoginAsync(model);
                return Ok(new ApiResponseDto<LoginResponseDto>(true, "Login successful", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return Unauthorized(new ApiResponseDto(false, ex.Message));
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var user = await _authService.GetCurrentUserAsync(userId);
                return Ok(new ApiResponseDto<UserDto>(true, "User retrieved successfully", user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponseDto>> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _authService.UpdateProfileAsync(userId, model);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Profile update failed"));

                return Ok(new ApiResponseDto(true, "Profile updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update profile error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponseDto>> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _authService.ChangePasswordAsync(userId, model);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Password change failed"));

                return Ok(new ApiResponseDto(true, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Change password error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        private Dictionary<string, string> GetModelErrors()
        {
            return ModelState
                .Where(ms => ms.Value.Errors.Count > 0)
                .ToDictionary(
                    ms => ms.Key,
                    ms => string.Join("; ", ms.Value.Errors.Select(e => e.ErrorMessage)));
        }
    }
}

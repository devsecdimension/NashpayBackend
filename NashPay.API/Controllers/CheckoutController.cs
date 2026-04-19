using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NashPay.API.DTOs;
using NashPay.API.Services;

namespace NashPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICheckoutService checkoutService, ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        [HttpPost("initiate")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<CheckoutResponseDto>>> InitiateCheckout([FromBody] InitiateCheckoutDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                // Validate input
                if (string.IsNullOrEmpty(model.OrderId) || model.Amount <= 0)
                    return BadRequest(new ApiResponseDto(false, "Invalid order details"));

                var result = await _checkoutService.InitiateCheckoutAsync(userId, model);
                return Ok(new ApiResponseDto<CheckoutResponseDto>(true, "Checkout initiated successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initiate checkout error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("{checkoutSessionId}")]
        public async Task<ActionResult<ApiResponseDto<CheckoutSessionDto>>> GetCheckoutSession(string checkoutSessionId)
        {
            try
            {
                if (string.IsNullOrEmpty(checkoutSessionId))
                    return BadRequest(new ApiResponseDto(false, "Checkout session ID required"));

                var session = await _checkoutService.GetCheckoutSessionAsync(checkoutSessionId);
                return Ok(new ApiResponseDto<CheckoutSessionDto>(true, "Checkout session retrieved", session));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponseDto(false, "Checkout session not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get checkout session error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{checkoutSessionId}/complete")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CompleteCheckout(
            string checkoutSessionId,
            [FromBody] VerifyCheckoutDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(checkoutSessionId))
                    return BadRequest(new ApiResponseDto(false, "Checkout session ID required"));

                var result = await _checkoutService.CompleteCheckoutAsync(checkoutSessionId, model.TransactionId);
                return Ok(new ApiResponseDto<bool>(true, "Checkout completed successfully", result));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponseDto(false, "Checkout session not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Complete checkout error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{checkoutSessionId}/cancel")]
        public async Task<ActionResult<ApiResponseDto<bool>>> CancelCheckout(string checkoutSessionId)
        {
            try
            {
                if (string.IsNullOrEmpty(checkoutSessionId))
                    return BadRequest(new ApiResponseDto(false, "Checkout session ID required"));

                var result = await _checkoutService.CancelCheckoutAsync(checkoutSessionId);
                return Ok(new ApiResponseDto<bool>(true, "Checkout cancelled successfully", result));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponseDto(false, "Checkout session not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cancel checkout error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("merchant/checkouts")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<CheckoutSessionDto>>>> GetMerchantCheckouts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var checkouts = await _checkoutService.GetMerchantCheckoutsAsync(userId, pageNumber, pageSize);
                return Ok(new ApiResponseDto<IEnumerable<CheckoutSessionDto>>(true, "Checkouts retrieved", checkouts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get merchant checkouts error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

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
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<ApiResponseDto<PaymentResponseDto>>> InitiatePayment([FromBody] InitiatePaymentDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var result = await _paymentService.InitiatePaymentAsync(userId, model);
                return Ok(new ApiResponseDto<PaymentResponseDto>(true, "Payment initiated successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initiate payment error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("{transactionId}")]
        public async Task<ActionResult<ApiResponseDto<TransactionDto>>> GetTransaction(string transactionId)
        {
            try
            {
                var transaction = await _paymentService.GetTransactionAsync(transactionId);
                return Ok(new ApiResponseDto<TransactionDto>(true, "Transaction retrieved successfully", transaction));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get transaction error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("user/transactions")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<TransactionDto>>>> GetUserTransactions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var transactions = await _paymentService.GetUserTransactionsAsync(userId, pageNumber, pageSize);
                return Ok(new ApiResponseDto<IEnumerable<TransactionDto>>(true, "Transactions retrieved successfully", transactions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get transactions error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{transactionId}/verify")]
        public async Task<ActionResult<ApiResponseDto<PaymentResponseDto>>> VerifyPayment(string transactionId)
        {
            try
            {
                var result = await _paymentService.VerifyPaymentAsync(transactionId);
                return Ok(new ApiResponseDto<PaymentResponseDto>(true, "Payment verified successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Verify payment error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpPost("{transactionId}/refund")]
        public async Task<ActionResult<ApiResponseDto>> RefundTransaction(string transactionId, [FromBody] string reason)
        {
            try
            {
                var result = await _paymentService.RefundTransactionAsync(transactionId, reason);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Refund failed"));

                return Ok(new ApiResponseDto(true, "Refund processed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refund payment error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

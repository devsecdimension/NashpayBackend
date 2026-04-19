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
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<WalletDto>>> GetWallet()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var wallet = await _walletService.GetWalletAsync(userId);
                return Ok(new ApiResponseDto<WalletDto>(true, "Wallet retrieved successfully", wallet));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get wallet error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("balance")]
        public async Task<ActionResult<ApiResponseDto<WalletBalanceDto>>> GetBalance()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var balance = await _walletService.GetBalanceAsync(userId);
                return Ok(new ApiResponseDto<WalletBalanceDto>(true, "Balance retrieved successfully", balance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get balance error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<LedgerDto>>>> GetLedger()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User not found"));

                var ledger = await _walletService.GetLedgerEntriesAsync(userId);
                return Ok(new ApiResponseDto<IEnumerable<LedgerDto>>(true, "Ledger retrieved successfully", ledger));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get ledger error");
                return BadRequest(new ApiResponseDto(false, ex.Message));
            }
        }
    }
}

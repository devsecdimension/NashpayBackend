using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using NashPay.API.DTOs;
using NashPay.API.Services;

namespace NashPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _webhookService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<WebhookDetailDto>>> RegisterWebhook([FromBody] RegisterWebhookDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User authentication failed"));

                if (string.IsNullOrEmpty(model.Url) || !Uri.IsWellFormedUriString(model.Url, UriKind.Absolute))
                    return BadRequest(new ApiResponseDto(false, "Please provide a valid absolute URL"));

                if (model.Events == null || model.Events.Count == 0)
                    return BadRequest(new ApiResponseDto(false, "Select at least one event type (e.g., payment.success)"));

                var result = await _webhookService.RegisterWebhookAsync(userId, model);
                return Ok(new ApiResponseDto<WebhookDetailDto>(true, "Webhook registered successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register webhook error");
                return BadRequest(new ApiResponseDto(false, "Could not register webhook. URL might be unreachable."));
            }
        }

        [HttpGet("list")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<WebhookDetailDto>>>> GetUserWebhooks()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var webhooks = await _webhookService.GetUserWebhooksAsync(userId);
                return Ok(new ApiResponseDto<IEnumerable<WebhookDetailDto>>(true, "Active webhooks retrieved", webhooks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get user webhooks error");
                return BadRequest(new ApiResponseDto(false, "Error fetching your webhooks."));
            }
        }

        [HttpGet("{webhookId}")]
        public async Task<ActionResult<ApiResponseDto<WebhookDetailDto>>> GetWebhook(string webhookId)
        {
            try
            {
                var webhook = await _webhookService.GetWebhookAsync(webhookId);
                return Ok(new ApiResponseDto<WebhookDetailDto>(true, "Webhook found", webhook));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponseDto(false, "The requested webhook configuration does not exist"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get webhook error");
                return BadRequest(new ApiResponseDto(false, "Internal server error."));
            }
        }

        [HttpPut("{webhookId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> UpdateWebhook(string webhookId, [FromBody] UpdateWebhookDto model)
        {
            try
            {
                var result = await _webhookService.UpdateWebhookAsync(webhookId, model);
                return Ok(new ApiResponseDto<bool>(true, "Webhook settings updated", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update webhook error");
                return BadRequest(new ApiResponseDto(false, "Update failed. Check your payload."));
            }
        }

        [HttpDelete("{webhookId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteWebhook(string webhookId)
        {
            try
            {
                var result = await _webhookService.DeleteWebhookAsync(webhookId);
                return Ok(new ApiResponseDto<bool>(true, "Webhook endpoint removed", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete webhook error");
                return BadRequest(new ApiResponseDto(false, "Deletion failed."));
            }
        }

        [HttpPost("{webhookId}/toggle")]
        public async Task<ActionResult<ApiResponseDto<bool>>> ToggleWebhook(string webhookId, [FromBody] Dictionary<string, bool> model)
        {
            try
            {
                if (!model.TryGetValue("isActive", out bool isActive))
                    return BadRequest(new ApiResponseDto(false, "Field 'isActive' is required"));

                var result = await _webhookService.ToggleWebhookAsync(webhookId, isActive);
                return Ok(new ApiResponseDto<bool>(true, $"Webhook {(isActive ? "enabled" : "disabled")}", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toggle error");
                return BadRequest(new ApiResponseDto(false, "Failed to toggle webhook state."));
            }
        }

        [HttpGet("{webhookId}/logs")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<WebhookLogDetailDto>>>> GetWebhookLogs(
            string webhookId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var logs = await _webhookService.GetWebhookLogsAsync(webhookId, pageNumber, pageSize);
                return Ok(new ApiResponseDto<IEnumerable<WebhookLogDetailDto>>(true, "Delivery logs retrieved", logs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log retrieval error");
                return BadRequest(new ApiResponseDto(false, "Unable to fetch logs."));
            }
        }

        // ===== External Webhook Receiver =====
        [HttpPost("receive")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<bool>>> ReceiveWebhook()
        {
            try
            {
                var body = await new StreamReader(Request.Body).ReadToEndAsync();
                var signature = Request.Headers["X-NashPay-Signature"].ToString();
                var webhookId = Request.Headers["X-NashPay-Webhook-ID"].ToString();

                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(webhookId))
                    return BadRequest(new ApiResponseDto(false, "Security headers missing"));

                _logger.LogInformation($"Incoming webhook: {webhookId}");

                // Logic implementation in Service layer recommended for signature verification
                // await _webhookService.ProcessIncomingWebhook(webhookId, body, signature);

                return Ok(new ApiResponseDto<bool>(true, "Acknowledge", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receiver error");
                return BadRequest(new ApiResponseDto(false, "Processing failed"));
            }
        }

        [HttpPost("test")]
        public async Task<ActionResult<ApiResponseDto<bool>>> TestWebhook([FromBody] WebhookTestDto model)
        {
            try
            {
                var testData = model.TestData ?? new Dictionary<string, object>
                {
                    { "event", model.EventType },
                    { "timestamp", DateTime.UtcNow },
                    { "mode", "test" }
                };

                await _webhookService.SendWebhookAsync(model.EventType, testData);
                return Ok(new ApiResponseDto<bool>(true, "Test notification dispatched", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test trigger error");
                return BadRequest(new ApiResponseDto(false, "Test failed. Endpoint might be down."));
            }
        }
    }
}
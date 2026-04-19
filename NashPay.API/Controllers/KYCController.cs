using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using NashPay.API.DTOs;
using NashPay.API.Services;

namespace NashPay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Default login required for all methods
    public class KYCController : ControllerBase
    {
        private readonly IKYCService _kycService;
        private readonly ILogger<KYCController> _logger;

        public KYCController(IKYCService kycService, ILogger<KYCController> logger)
        {
            _kycService = kycService;
            _logger = logger;
        }

        // Merchants/Users can upload their own documents
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponseDto<KYCDocumentDto>>> UploadDocument([FromForm] UploadKYCDocumentDto model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiResponseDto(false, "User session not found"));

                var result = await _kycService.UploadDocumentAsync(userId, model);
                return Ok(new ApiResponseDto<KYCDocumentDto>(true, "Document uploaded successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload document error for user");
                return BadRequest(new ApiResponseDto(false, "An error occurred while uploading. Please ensure file format is correct."));
            }
        }

        // Restricting these to Admin only
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<KYCDocumentDto>>>> GetUserDocuments(string userId)
        {
            try
            {
                var result = await _kycService.GetUserDocumentsAsync(userId);
                return Ok(new ApiResponseDto<IEnumerable<KYCDocumentDto>>(true, "Documents retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get documents error");
                return BadRequest(new ApiResponseDto(false, "Could not retrieve documents."));
            }
        }

        [HttpGet("{documentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<KYCDocumentDto>>> GetDocument(int documentId)
        {
            try
            {
                var result = await _kycService.GetDocumentAsync(documentId);
                return Ok(new ApiResponseDto<KYCDocumentDto>(true, "Document retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get document error");
                return BadRequest(new ApiResponseDto(false, "Document not found."));
            }
        }

        [HttpPost("{documentId}/verify")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto>> VerifyDocument(int documentId, [FromBody] VerifyKYCDocumentDto model)
        {
            try
            {
                var result = await _kycService.VerifyDocumentAsync(documentId, model.IsApproved, model.RejectionReason);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Verification process failed."));

                return Ok(new ApiResponseDto(true, "Document status updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Verify document error");
                return BadRequest(new ApiResponseDto(false, "Error updating verification status."));
            }
        }

        [HttpGet("pending/list")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<KYCDocumentDto>>>> GetPendingDocuments()
        {
            try
            {
                var result = await _kycService.GetPendingDocumentsAsync();
                return Ok(new ApiResponseDto<IEnumerable<KYCDocumentDto>>(true, "Pending documents retrieved", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get pending documents error");
                return BadRequest(new ApiResponseDto(false, "Failed to load pending queue."));
            }
        }

        [HttpPut("{userId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto>> UpdateKYCStatus(string userId, [FromBody] string status)
        {
            try
            {
                var result = await _kycService.UpdateKYCStatusAsync(userId, status);
                if (!result)
                    return BadRequest(new ApiResponseDto(false, "Global KYC status update failed"));

                return Ok(new ApiResponseDto(true, "User KYC status finalized successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update KYC status error");
                return BadRequest(new ApiResponseDto(false, "Internal error updating user status."));
            }
        }
    }
}
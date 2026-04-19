using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace NashPay.API.Services
{
    public interface IKYCService
    {
        Task<KYCDocumentDto> UploadDocumentAsync(string userId, UploadKYCDocumentDto model);
        Task<IEnumerable<KYCDocumentDto>> GetUserDocumentsAsync(string userId);
        Task<KYCDocumentDto> GetDocumentAsync(int documentId);
        Task<bool> VerifyDocumentAsync(int documentId, bool isApproved, string? rejectionReason = null);
        Task<IEnumerable<KYCDocumentDto>> GetPendingDocumentsAsync();
        Task<bool> UpdateKYCStatusAsync(string userId, string status);
    }

    public class KYCService : IKYCService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<KYCService> _logger;

        public KYCService(
            AppDbContext context,
            UserManager<User> userManager,
            ILogger<KYCService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<KYCDocumentDto> UploadDocumentAsync(string userId, UploadKYCDocumentDto model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                // TODO: Implement file upload to cloud storage (AWS S3, Azure Blob, etc)
                // For now, store file path reference
                string documentPath = Path.Combine("uploads", "kyc", userId, Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName));

                var kycDocument = new KYCDocument
                {
                    UserId = userId,
                    DocumentType = model.DocumentType,
                    DocumentUrl = documentPath,
                    FileName = model.File.FileName,
                    Status = "Pending",
                    UploadedAt = DateTime.UtcNow
                };

                _context.KYCDocuments.Add(kycDocument);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"KYC document uploaded for user {userId}");

                return MapToKYCDocumentDto(kycDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KYC document upload error");
                throw new Exception($"Document upload failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<KYCDocumentDto>> GetUserDocumentsAsync(string userId)
        {
            var documents = await _context.KYCDocuments
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return documents.Select(MapToKYCDocumentDto);
        }

        public async Task<KYCDocumentDto> GetDocumentAsync(int documentId)
        {
            var document = await _context.KYCDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                throw new Exception("Document not found");

            return MapToKYCDocumentDto(document);
        }

        public async Task<bool> VerifyDocumentAsync(int documentId, bool isApproved, string? rejectionReason = null)
        {
            try
            {
                var document = await _context.KYCDocuments
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document == null)
                    throw new Exception("Document not found");

                if (isApproved)
                {
                    document.Status = "Verified";
                    document.VerifiedAt = DateTime.UtcNow;
                    
                    // Check if all documents are verified
                    var pendingDocs = await _context.KYCDocuments
                        .Where(d => d.UserId == document.UserId && d.Status != "Verified")
                        .CountAsync();

                    if (pendingDocs == 1) // This is the last pending document
                    {
                        // Update user KYC status
                        if (document.User != null)
                        {
                            document.User.KYCStatus = "Approved";
                            _context.Users.Update(document.User);
                        }
                    }
                }
                else
                {
                    document.Status = "Rejected";
                    document.RejectionReason = rejectionReason ?? "Not provided";
                    document.VerifiedAt = DateTime.UtcNow;
                }

                _context.KYCDocuments.Update(document);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"KYC document {documentId} verified");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "KYC verification error");
                throw new Exception($"Document verification failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<KYCDocumentDto>> GetPendingDocumentsAsync()
        {
            var documents = await _context.KYCDocuments
                .Where(d => d.Status == "Pending")
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return documents.Select(MapToKYCDocumentDto);
        }

        public async Task<bool> UpdateKYCStatusAsync(string userId, string status)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            user.KYCStatus = status;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        private KYCDocumentDto MapToKYCDocumentDto(KYCDocument document)
        {
            return new KYCDocumentDto
            {
                Id = document.Id,
                UserId = document.UserId,
                DocumentType = document.DocumentType,
                FileName = document.FileName,
                Status = document.Status,
                RejectionReason = document.RejectionReason,
                UploadedAt = document.UploadedAt,
                VerifiedAt = document.VerifiedAt
            };
        }
    }
}

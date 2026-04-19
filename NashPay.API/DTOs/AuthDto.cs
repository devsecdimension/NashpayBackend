using System.ComponentModel.DataAnnotations;

namespace NashPay.API.DTOs
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; }

        [Required, Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public string Role { get; set; } = "Merchant"; 
        
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? TaxId { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string KYCStatus { get; set; }
        public string? BusinessName { get; set; } // Nullable handle kiya
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // --- missing work for authservice.ts file  ---

    public class UpdateProfileDto
    {
        [Required]
        public string FullName { get; set; }
        [Required, Phone]
        public string PhoneNumber { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required, MinLength(8)]
        public string NewPassword { get; set; }
        [Required, Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
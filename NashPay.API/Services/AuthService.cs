using Microsoft.AspNetCore.Identity;
using NashPay.API.Data;
using NashPay.API.DTOs;
using NashPay.API.Models;
using NashPay.API.Helpers;

namespace NashPay.API.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> RegisterAsync(RegisterDto model);
        Task<LoginResponseDto> LoginAsync(LoginDto model);
        Task<UserDto> GetCurrentUserAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto model);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto model);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthService(
            UserManager<User> userManager,
            AppDbContext context,
            JwtTokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _context = context;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<LoginResponseDto> RegisterAsync(RegisterDto model)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                    throw new Exception("User with this email already exists");

                // Create new user
                var user = new User
                {
                    Email = model.Email,
                    UserName = model.Email,
                    FullName = model.FullName,
                    Role = model.Role,
                    EmailConfirmed = false,
                    PhoneNumberConfirmed = false
                };

                // If Merchant, add business info
                if (model.Role == "Merchant")
                {
                    user.BusinessName = model.BusinessName;
                    user.BusinessType = model.BusinessType;
                    user.RegistrationNumber = model.RegistrationNumber;
                    user.TaxId = model.TaxId;
                }

                // Create user with password
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

                // Create wallet for user
                var wallet = new Wallet
                {
                    UserId = user.Id,
                    Balance = 0,
                    Currency = "PKR",
                    Status = "Active"
                };
                _context.Wallets.Add(wallet);

                // Add default role
                await _userManager.AddToRoleAsync(user, model.Role);

                await _context.SaveChangesAsync();

                // Generate token
                var token = _tokenGenerator.GenerateToken(user);

                return new LoginResponseDto
                {
                    Token = token,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Registration failed: {ex.Message}");
            }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    throw new Exception("Invalid email or password");

                var result = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!result)
                    throw new Exception("Invalid email or password");

                if (!user.IsActive)
                    throw new Exception("Account is disabled");

                // Generate token
                var token = _tokenGenerator.GenerateToken(user);

                return new LoginResponseDto
                {
                    Token = token,
                    User = MapToUserDto(user)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}");
            }
        }

        public async Task<UserDto> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            return MapToUserDto(user);
        }

        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            if (model.NewPassword != model.ConfirmPassword)
                throw new Exception("Passwords do not match");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return true;
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                KYCStatus = user.KYCStatus,
                BusinessName = user.BusinessName,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

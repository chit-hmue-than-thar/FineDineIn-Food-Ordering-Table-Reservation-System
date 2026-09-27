using System;
using System.Linq;
using System.Threading.Tasks;
using FoodOrdering.Database;
using FoodOrdering.Domain.DTOs;
using FoodOrdering.Domain.Entities;
using FoodOrdering.Domain.Enums;
using FoodOrdering.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.Database.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, TblUser? User)> ValidateAndLoginAsync(LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return (false, "Email and Password are required.", null);
            }

            var emailLower = loginDto.Email.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);

            if (user == null)
            {
                return (false, "Invalid email address or password.", null);
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return (false, "Invalid email address or password.", null);
            }

            // Reject non-customer roles on customer portal
            if (user.Role != UserRole.Customer)
            {
                return (false, $"Access Denied. Account with role '{user.Role}' cannot log into the Customer Web Portal.", null);
            }

            return (true, "Authentication successful.", user);
        }

        public async Task<(bool Success, string Message, TblUser? User)> RegisterCustomerAsync(RegisterDto registerDto)
        {
            var emailLower = registerDto.Email.Trim().ToLower();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);

            if (existingUser != null)
            {
                return (false, "An account with this email address already exists.", null);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var now = DateTime.UtcNow.AddHours(6).AddMinutes(30);

            var newUser = new TblUser
            {
                Id = Guid.NewGuid(),
                Name = registerDto.Name.Trim(),
                Email = emailLower,
                Phone = registerDto.Phone.Trim(),
                PasswordHash = passwordHash,
                Role = UserRole.Customer,
                CreatedAt = now
            };

            var userCart = new TblCart
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                CreatedAt = now
            };

            await _context.Users.AddAsync(newUser);
            await _context.Carts.AddAsync(userCart);
            await _context.SaveChangesAsync();

            return (true, "Account created successfully.", newUser);
        }

        public async Task<(bool Success, string Message, string? GeneratedOtp)> InitiateForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var emailLower = forgotPasswordDto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);

            if (user == null)
            {
                return (false, "No account found associated with this email address.", null);
            }

            // Generate 6-digit OTP
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            var mmTime = DateTime.UtcNow.AddHours(6).AddMinutes(30);

            user.OtpCode = otp;
            user.OtpExpiry = mmTime.AddMinutes(15);
            user.UpdatedAt = mmTime;

            await _context.SaveChangesAsync();

            return (true, $"OTP generated successfully. (Demo Code: {otp})", otp);
        }

        public async Task<(bool Success, string Message)> VerifyOtpAndResetPasswordAsync(VerifyOtpDto verifyOtpDto)
        {
            var emailLower = verifyOtpDto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailLower);

            if (user == null)
            {
                return (false, "User account not found.");
            }

            var mmTime = DateTime.UtcNow.AddHours(6).AddMinutes(30);
            if (string.IsNullOrEmpty(user.OtpCode) || user.OtpCode != verifyOtpDto.OtpCode.Trim())
            {
                return (false, "Invalid OTP code provided.");
            }

            if (!user.OtpExpiry.HasValue || user.OtpExpiry.Value < mmTime)
            {
                return (false, "The OTP code has expired. Please request a new code.");
            }

            // Reset password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(verifyOtpDto.NewPassword);
            user.OtpCode = null;
            user.OtpExpiry = null;
            user.UpdatedAt = mmTime;

            await _context.SaveChangesAsync();

            return (true, "Password has been successfully reset. You can now log in with your new password.");
        }
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(bool Success, string Message)> UpdateUserProfileAsync(Guid userId, string name, string phone)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return (false, "User not found.");

            user.Name = name.Trim();
            user.Phone = phone.Trim();
            user.UpdatedAt = DateTime.UtcNow.AddHours(6).AddMinutes(30);

            await _context.SaveChangesAsync();
            return (true, "Profile updated successfully.");
        }
    }
}

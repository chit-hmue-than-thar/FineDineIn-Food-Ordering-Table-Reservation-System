using System;
using System.Threading.Tasks;
using FoodOrdering.Domain.DTOs;
using FoodOrdering.Domain.Entities;

namespace FoodOrdering.Domain.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, TblUser? User)> ValidateAndLoginAsync(LoginDto loginDto);
        Task<(bool Success, string Message, TblUser? User)> RegisterCustomerAsync(RegisterDto registerDto);
        Task<(bool Success, string Message, string? GeneratedOtp)> InitiateForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<(bool Success, string Message)> VerifyOtpAndResetPasswordAsync(VerifyOtpDto verifyOtpDto);
    }

    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
        Task<(bool Success, string Message)> UpdateUserProfileAsync(Guid userId, string name, string phone);
    }
}

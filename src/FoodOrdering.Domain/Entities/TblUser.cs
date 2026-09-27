using System;
using System.Collections.Generic;
using FoodOrdering.Domain.Enums;

namespace FoodOrdering.Domain.Entities
{
    public class TblUser : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;

        // OTP fields for reset password simulation
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }

        // Navigation properties
        public virtual ICollection<TblRefreshToken> RefreshTokens { get; set; } = new List<TblRefreshToken>();
        public virtual ICollection<TblCart> Carts { get; set; } = new List<TblCart>();
        public virtual ICollection<TblSale> Sales { get; set; } = new List<TblSale>();
        public virtual ICollection<TblReservation> Reservations { get; set; } = new List<TblReservation>();
        public virtual ICollection<TblNotification> Notifications { get; set; } = new List<TblNotification>();
        public virtual ICollection<TblWishlist> Wishlists { get; set; } = new List<TblWishlist>();
    }

    public class TblRefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string RefreshTokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public virtual TblUser? User { get; set; }
    }
}

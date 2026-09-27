using System;
using System.Collections.Generic;
using FoodOrdering.Domain.Enums;

namespace FoodOrdering.Domain.Entities
{
    public class TblReservation : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid TableId { get; set; }
        public DateTime ReservationDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // e.g. "18:00 - 20:00"
        public int PartySize { get; set; } = 2;
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public string SpecialRequests { get; set; } = string.Empty;

        public virtual TblUser? User { get; set; }
        public virtual TblTable? Table { get; set; }
    }

    public class TblNotification : BaseEntity
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; } = NotificationType.System;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;

        public virtual TblUser? User { get; set; }
    }

    public class TblPromotion : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string PromoCode { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
        public decimal Value { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string BannerUrl { get; set; } = string.Empty;
    }

    public class TblWishlist
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid FoodItemId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual TblUser? User { get; set; }
        public virtual TblFoodItem? FoodItem { get; set; }
    }

    public class TblDelivery : BaseEntity
    {
        public Guid SaleId { get; set; }
        public Guid? DeliveryPersonId { get; set; }
        public string DeliveryStatus { get; set; } = "Assigned";
        public string Address { get; set; } = string.Empty;
        public DateTime? EstimatedDeliveryTime { get; set; }

        public virtual TblSale? Sale { get; set; }
        public virtual TblUser? DeliveryPerson { get; set; }
    }
}

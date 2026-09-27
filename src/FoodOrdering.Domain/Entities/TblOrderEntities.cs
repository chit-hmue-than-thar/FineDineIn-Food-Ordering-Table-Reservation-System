using System;
using System.Collections.Generic;
using FoodOrdering.Domain.Enums;

namespace FoodOrdering.Domain.Entities
{
    public class TblCart : BaseEntity
    {
        public Guid UserId { get; set; }

        public virtual TblUser? User { get; set; }
        public virtual ICollection<TblCartItem> CartItems { get; set; } = new List<TblCartItem>();
    }

    public class TblCartItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CartId { get; set; }
        public Guid FoodItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public virtual TblCart? Cart { get; set; }
        public virtual TblFoodItem? FoodItem { get; set; }
    }

    public class TblSale : BaseEntity
    {
        public string VoucherNo { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid? TableId { get; set; }
        public Guid? PromotionId { get; set; }
        public SaleType Type { get; set; } = SaleType.DineIn;
        public SaleStatus Status { get; set; } = SaleStatus.Placed;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal DeliveryFee { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public string? PaymentScreenshotUrl { get; set; }
        public string? Notes { get; set; }

        public virtual TblUser? User { get; set; }
        public virtual TblTable? Table { get; set; }
        public virtual TblPromotion? Promotion { get; set; }
        public virtual ICollection<TblSaleDetail> SaleDetails { get; set; } = new List<TblSaleDetail>();
        public virtual ICollection<TblDelivery> Deliveries { get; set; } = new List<TblDelivery>();
    }

    public class TblSaleDetail
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SaleId { get; set; }
        public Guid FoodItemId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public decimal PricePerItem { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }

        public virtual TblSale? Sale { get; set; }
        public virtual TblFoodItem? FoodItem { get; set; }
    }
}

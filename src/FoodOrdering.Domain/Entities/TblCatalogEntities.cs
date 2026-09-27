using System;
using System.Collections.Generic;
using FoodOrdering.Domain.Enums;

namespace FoodOrdering.Domain.Entities
{
    public class TblFoodCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;

        public virtual ICollection<TblFoodItem> FoodItems { get; set; } = new List<TblFoodItem>();
    }

    public class TblFoodItem : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int EstimatedPrepTimeMinutes { get; set; } = 20;
        public double Rating { get; set; } = 5.0;
        public string MainIngredients { get; set; } = string.Empty;
        public FoodSizeOption SizeOptions { get; set; } = FoodSizeOption.Standard;
        public decimal DeliveryFee { get; set; } = 0;
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        public virtual TblFoodCategory? Category { get; set; }
        public virtual ICollection<TblCartItem> CartItems { get; set; } = new List<TblCartItem>();
        public virtual ICollection<TblSaleDetail> SaleDetails { get; set; } = new List<TblSaleDetail>();
        public virtual ICollection<TblWishlist> Wishlists { get; set; } = new List<TblWishlist>();
    }

    public class TblTable : BaseEntity
    {
        public string TableNumber { get; set; } = string.Empty;
        public int Capacity { get; set; } = 2;
        public TableLocationType LocationType { get; set; } = TableLocationType.Indoor;
        public TableStatus Status { get; set; } = TableStatus.Available;
        public string Description { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;

        public virtual ICollection<TblReservation> Reservations { get; set; } = new List<TblReservation>();
        public virtual ICollection<TblSale> Sales { get; set; } = new List<TblSale>();
    }
}

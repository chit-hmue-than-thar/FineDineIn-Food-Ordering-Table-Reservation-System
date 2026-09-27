using System;
using System.Collections.Generic;
using FoodOrdering.Domain.Entities;
using FoodOrdering.Domain.Enums;

namespace FoodOrdering.CustomerWeb.Models
{
    public class CartViewModel
    {
        public Guid CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<TblTable> AvailableTables { get; set; } = new List<TblTable>();
    }

    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid FoodItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Amount => Price * Quantity;
    }

    public class PaymentViewModel
    {
        public Guid SaleId { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public SaleStatus Status { get; set; }
        public string? PaymentScreenshotUrl { get; set; }
        public string KPayNumber { get; set; } = "09-791234567";
        public string KPayAccountName { get; set; } = "U Julian Vance (FineDineIn)";
        public string WavePayNumber { get; set; } = "09-791234567";
        public string WavePayAccountName { get; set; } = "U Julian Vance (FineDineIn)";
    }

    public class SeatMapViewModel
    {
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public string SelectedTimeSlot { get; set; } = "18:00 - 20:00";
        public int PartySize { get; set; } = 2;
        public TableLocationType? LocationFilter { get; set; }
        public List<TableSeatDto> Tables { get; set; } = new List<TableSeatDto>();
        public List<TblReservation> CustomerReservations { get; set; } = new List<TblReservation>();
    }

    public class TableSeatDto
    {
        public Guid TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public TableLocationType LocationType { get; set; }
        public bool IsAvailableForSlot { get; set; } = true;
        public string Description { get; set; } = string.Empty;
    }

    public class OrderTrackingViewModel
    {
        public TblSale Sale { get; set; } = new TblSale();
        public List<TblSaleDetail> Details { get; set; } = new List<TblSaleDetail>();
        public int ProgressPercentage { get; set; }
    }

    public class ReservationWizardViewModel
    {
        // Step 1: Customer Contact Details
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Step 2: Date & Time Slot Selection
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public string SelectedTimeSlot { get; set; } = "18:00 - 20:00";
        public int PartySize { get; set; } = 2;
        public Guid? SelectedTableId { get; set; }
        public TableLocationType? LocationFilter { get; set; }
        public List<TableSeatDto> Tables { get; set; } = new List<TableSeatDto>();

        // Step 3: Payment Bank Options
        public string SelectedBank { get; set; } = "KBZPay";
        public string KPayNumber { get; set; } = "09-791234567";
        public string KPayAccountName { get; set; } = "U Julian Vance (FineDineIn)";
        public string WavePayNumber { get; set; } = "09-791234567";
        public string WavePayAccountName { get; set; } = "U Julian Vance (FineDineIn)";
        public decimal DepositAmount { get; set; } = 50000;

        // Step 4: Receipt Upload & Terms
        public string? PaymentScreenshotUrl { get; set; }
        public bool AcceptTerms { get; set; }

        // Step 5: Final Outcome Receipt
        public Guid? CreatedReservationId { get; set; }
        public string? VoucherNo { get; set; }
        public bool IsSuccess { get; set; }
        public List<TblReservation> CustomerReservations { get; set; } = new List<TblReservation>();
    }
}

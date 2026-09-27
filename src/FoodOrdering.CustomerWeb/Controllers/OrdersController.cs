using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodOrdering.Database;
using FoodOrdering.CustomerWeb.Models;
using FoodOrdering.Domain.Entities;
using FoodOrdering.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.CustomerWeb.Controllers
{
    [Authorize(Roles = "Customer")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(Guid? tableId, string? notes)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return RedirectToAction("Login", "Auth");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add food items before placing an order.";
                return RedirectToAction("Index", "Cart");
            }

            // Transaction-safe order creation
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow.AddHours(6).AddMinutes(30);
                var subTotal = cart.CartItems.Sum(ci => (ci.FoodItem?.Price ?? 0) * ci.Quantity);
                var discount = subTotal * 0.10m; // 10% promo
                var total = subTotal - discount;

                // Unique Voucher No
                var randomSuffix = new Random().Next(1000, 9999);
                var voucherNo = $"VCH-{now:yyyyMMdd}-{randomSuffix}";

                var newSale = new TblSale
                {
                    Id = Guid.NewGuid(),
                    VoucherNo = voucherNo,
                    UserId = userId,
                    TableId = tableId,
                    Type = SaleType.DineIn,
                    Status = SaleStatus.PaymentPending,
                    SubTotal = subTotal,
                    DiscountAmount = discount,
                    DeliveryFee = 0,
                    TotalAmount = total,
                    PurchaseDate = now,
                    Notes = notes,
                    CreatedAt = now
                };

                await _context.Sales.AddAsync(newSale);

                foreach (var ci in cart.CartItems)
                {
                    var detail = new TblSaleDetail
                    {
                        Id = Guid.NewGuid(),
                        SaleId = newSale.Id,
                        FoodItemId = ci.FoodItemId,
                        FoodName = ci.FoodItem?.Title ?? "Food Item",
                        PricePerItem = ci.FoodItem?.Price ?? 0,
                        Quantity = ci.Quantity,
                        Amount = (ci.FoodItem?.Price ?? 0) * ci.Quantity
                    };
                    await _context.SaleDetails.AddAsync(detail);
                }

                // Clear cart items
                _context.CartItems.RemoveRange(cart.CartItems);

                // Add System Notification
                var notification = new TblNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = NotificationType.OrderUpdate,
                    Title = "Order Placed Successfully",
                    Message = $"Your Dine-in order #{voucherNo} has been placed. Please complete your payment.",
                    CreatedAt = now
                };
                await _context.Notifications.AddAsync(notification);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Order #{voucherNo} placed! Please upload your payment receipt.";
                return RedirectToAction("Index", "Payment", new { saleId = newSale.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Failed to place order: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetCurrentUserId();
            var orders = await _context.Sales
                .Include(s => s.SaleDetails)
                .Include(s => s.Table)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var userId = GetCurrentUserId();
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.FoodItem)
                .Include(s => s.Table)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (sale == null) return NotFound("Order not found.");

            int progress = sale.Status switch
            {
                SaleStatus.Placed => 20,
                SaleStatus.PaymentPending => 35,
                SaleStatus.Paid => 50,
                SaleStatus.Preparing => 70,
                SaleStatus.Ready => 85,
                SaleStatus.Completed => 100,
                _ => 10
            };

            var viewModel = new OrderTrackingViewModel
            {
                Sale = sale,
                Details = sale.SaleDetails.ToList(),
                ProgressPercentage = progress
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Receipt(Guid id)
        {
            var userId = GetCurrentUserId();
            var sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .Include(s => s.User)
                .Include(s => s.Table)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (sale == null) return NotFound("Receipt not found.");

            return View(sale);
        }
    }
}

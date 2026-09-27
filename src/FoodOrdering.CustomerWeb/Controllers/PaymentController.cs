using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodOrdering.Database;
using FoodOrdering.CustomerWeb.Models;
using FoodOrdering.Domain.Entities;
using FoodOrdering.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.CustomerWeb.Controllers
{
    [Authorize(Roles = "Customer")]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PaymentController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid saleId)
        {
            var userId = GetCurrentUserId();
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.Id == saleId && s.UserId == userId);

            if (sale == null) return NotFound("Order not found.");

            var viewModel = new PaymentViewModel
            {
                SaleId = sale.Id,
                VoucherNo = sale.VoucherNo,
                TotalAmount = sale.TotalAmount,
                Status = sale.Status,
                PaymentScreenshotUrl = sale.PaymentScreenshotUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadScreenshot(Guid saleId, IFormFile paymentScreenshot)
        {
            var userId = GetCurrentUserId();
            var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == saleId && s.UserId == userId);

            if (sale == null) return NotFound("Order not found.");

            if (paymentScreenshot == null || paymentScreenshot.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a payment receipt screenshot to upload.";
                return RedirectToAction("Index", new { saleId = sale.Id });
            }

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "payments");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{sale.VoucherNo}_{Guid.NewGuid().ToString().Substring(0, 8)}{Path.GetExtension(paymentScreenshot.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await paymentScreenshot.CopyToAsync(fileStream);
                }

                var mmTime = DateTime.UtcNow.AddHours(6).AddMinutes(30);
                sale.PaymentScreenshotUrl = $"/uploads/payments/{uniqueFileName}";
                sale.Status = SaleStatus.PaymentPending;
                sale.UpdatedAt = mmTime;

                var notification = new TblNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = NotificationType.OrderUpdate,
                    Title = "Payment Receipt Submitted",
                    Message = $"Your payment screenshot for #{sale.VoucherNo} was received and is pending verification.",
                    CreatedAt = mmTime
                };
                await _context.Notifications.AddAsync(notification);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Payment screenshot uploaded successfully! Pending admin verification.";
                return RedirectToAction("Index", new { saleId = sale.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to upload screenshot: " + ex.Message;
                return RedirectToAction("Index", new { saleId = sale.Id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulateVerify(Guid saleId)
        {
            var userId = GetCurrentUserId();
            var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == saleId && s.UserId == userId);

            if (sale == null) return NotFound("Order not found.");

            var mmTime = DateTime.UtcNow.AddHours(6).AddMinutes(30);
            sale.Status = SaleStatus.Paid;
            sale.UpdatedAt = mmTime;

            var notification = new TblNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = NotificationType.OrderUpdate,
                Title = "Payment Verified!",
                Message = $"Payment for Order #{sale.VoucherNo} has been verified. Kitchen is preparing your dishes!",
                CreatedAt = mmTime
            };
            await _context.Notifications.AddAsync(notification);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment simulated & verified! Order is now marked as Paid & Preparing.";
            return RedirectToAction("Receipt", "Orders", new { id = sale.Id });
        }
    }
}

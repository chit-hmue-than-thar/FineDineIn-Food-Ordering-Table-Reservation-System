using System;
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
    public class ReservationController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date = null, string? timeSlot = null, int partySize = 2, TableLocationType? location = null)
        {
            var userId = GetCurrentUserId();
            var targetDate = date?.Date ?? DateTime.Today;
            var slot = string.IsNullOrWhiteSpace(timeSlot) ? "18:00 - 20:00" : timeSlot.Trim();

            // Find existing reservations for date & slot
            var existingReservations = await _context.Reservations
                .Where(r => r.ReservationDate.Date == targetDate && r.TimeSlot == slot && r.Status != ReservationStatus.Cancelled)
                .ToListAsync();

            var reservedTableIds = existingReservations.Select(r => r.TableId).ToHashSet();

            var tablesQuery = _context.Tables.AsQueryable();
            if (location.HasValue)
            {
                tablesQuery = tablesQuery.Where(t => t.LocationType == location.Value);
            }

            var allTables = await tablesQuery.OrderBy(t => t.TableNumber).ToListAsync();

            var tableSeats = allTables.Select(t => new TableSeatDto
            {
                TableId = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                LocationType = t.LocationType,
                Description = t.Description,
                IsAvailableForSlot = !reservedTableIds.Contains(t.Id) && t.Capacity >= partySize
            }).ToList();

            var myReservations = await _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var viewModel = new SeatMapViewModel
            {
                SelectedDate = targetDate,
                SelectedTimeSlot = slot,
                PartySize = partySize,
                LocationFilter = location,
                Tables = tableSeats,
                CustomerReservations = myReservations
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(Guid tableId, DateTime reservationDate, string timeSlot, int partySize, string? specialRequests)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return RedirectToAction("Login", "Auth");

            var slot = timeSlot.Trim();
            var targetDate = reservationDate.Date;

            // Double-booking protection with DB Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var isConflict = await _context.Reservations
                    .AnyAsync(r => r.TableId == tableId && r.ReservationDate.Date == targetDate && r.TimeSlot == slot && r.Status != ReservationStatus.Cancelled);

                if (isConflict)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "This table has already been reserved for the selected date and time slot. Please choose another table or time.";
                    return RedirectToAction("Index", new { date = targetDate, timeSlot = slot, partySize });
                }

                var mmTime = DateTime.UtcNow.AddHours(6).AddMinutes(30);
                var newReservation = new TblReservation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TableId = tableId,
                    ReservationDate = targetDate,
                    TimeSlot = slot,
                    PartySize = partySize,
                    Status = ReservationStatus.Confirmed,
                    SpecialRequests = specialRequests ?? "",
                    CreatedAt = mmTime
                };

                await _context.Reservations.AddAsync(newReservation);

                var notification = new TblNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = NotificationType.ReservationUpdate,
                    Title = "Table Reserved!",
                    Message = $"Your table reservation for {targetDate:MMM dd, yyyy} ({slot}) has been confirmed.",
                    CreatedAt = mmTime
                };
                await _context.Notifications.AddAsync(notification);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Table reservation confirmed successfully!";
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Reservation failed: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyReservations()
        {
            var userId = GetCurrentUserId();
            var reservations = await _context.Reservations
                .Include(r => r.Table)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid reservationId)
        {
            var userId = GetCurrentUserId();
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation != null)
            {
                var mmTime = DateTime.UtcNow.AddHours(6).AddMinutes(30);
                reservation.Status = ReservationStatus.Cancelled;
                reservation.UpdatedAt = mmTime;

                var notification = new TblNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = NotificationType.ReservationUpdate,
                    Title = "Reservation Cancelled",
                    Message = $"Your table reservation for {reservation.ReservationDate:MMM dd, yyyy} was cancelled.",
                    CreatedAt = mmTime
                };
                await _context.Notifications.AddAsync(notification);

                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "Table reservation cancelled.";
            }

            return RedirectToAction("MyReservations");
        }
    }
}

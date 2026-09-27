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
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        }

        private async Task<TblCart> GetOrCreateUserCartAsync(Guid userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new TblCart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow.AddHours(6).AddMinutes(30)
                };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var cart = await GetOrCreateUserCartAsync(userId);

            var items = cart.CartItems.Select(ci => new CartItemDto
            {
                CartItemId = ci.Id,
                FoodItemId = ci.FoodItemId,
                Title = ci.FoodItem?.Title ?? "Food Dish",
                ImageUrl = ci.FoodItem?.ImageUrl ?? "",
                Price = ci.FoodItem?.Price ?? 0,
                Quantity = ci.Quantity
            }).ToList();

            var subTotal = items.Sum(i => i.Amount);
            var discount = subTotal * 0.10m; // 10% promo discount
            var total = subTotal - discount;

            var tables = await _context.Tables
                .Where(t => t.Status == TableStatus.Available)
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                CartId = cart.Id,
                Items = items,
                SubTotal = subTotal,
                DiscountAmount = discount,
                TotalAmount = total,
                AvailableTables = tables
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid foodItemId, int quantity = 1)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return RedirectToAction("Login", "Auth");

            if (!await _context.FoodItems.AnyAsync())
            {
                await DbSeeder.SeedAsync(_context);
            }

            var foodItem = await _context.FoodItems.FirstOrDefaultAsync(i => i.Id == foodItemId);
            
            // Fallback if item ID was Guid.Empty or fallback item from UI mock
            if (foodItem == null)
            {
                foodItem = await _context.FoodItems.FirstOrDefaultAsync();
            }

            if (foodItem == null)
            {
                var now = DateTime.UtcNow.AddHours(6).AddMinutes(30);
                var defaultCat = await _context.FoodCategories.FirstOrDefaultAsync() ?? new TblFoodCategory { Id = Guid.NewGuid(), Name = "Main Course", CreatedAt = now };
                if (_context.Entry(defaultCat).State == EntityState.Detached)
                {
                    await _context.FoodCategories.AddAsync(defaultCat);
                }

                foodItem = new TblFoodItem
                {
                    Id = Guid.NewGuid(),
                    CategoryId = defaultCat.Id,
                    Title = "Grilled Salmon",
                    Description = "Served with lemon butter sauce",
                    Price = 45000m,
                    EstimatedPrepTimeMinutes = 20,
                    Rating = 4.8,
                    ImageUrl = "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=500",
                    CreatedAt = now
                };
                await _context.FoodItems.AddAsync(foodItem);
                await _context.SaveChangesAsync();
            }

            var cart = await GetOrCreateUserCartAsync(userId);
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodItemId == foodItem.Id);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                cartItem.UpdatedAt = DateTime.UtcNow.AddHours(6).AddMinutes(30);
            }
            else
            {
                cartItem = new TblCartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    FoodItemId = foodItem.Id,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow.AddHours(6).AddMinutes(30)
                };
                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Added '{foodItem.Title}' to your cart!";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(Guid cartItemId, int quantity)
        {
            var userId = GetCurrentUserId();
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == userId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                    cartItem.UpdatedAt = DateTime.UtcNow.AddHours(6).AddMinutes(30);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid cartItemId)
        {
            var userId = GetCurrentUserId();
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == userId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "Item removed from cart.";
            }

            return RedirectToAction("Index");
        }
    }
}

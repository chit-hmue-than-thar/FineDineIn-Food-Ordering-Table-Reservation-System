using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodOrdering.Database;
using FoodOrdering.CustomerWeb.Models;
using FoodOrdering.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.CustomerWeb.Controllers
{
    public class CatalogController : Controller
    {
        private readonly AppDbContext _context;

        public CatalogController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid? categoryId = null, string? search = null)
        {
            var categories = await _context.FoodCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var query = _context.FoodItems
                .Include(i => i.Category)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(i => i.Title.ToLower().Contains(searchLower) || i.Description.ToLower().Contains(searchLower));
            }

            var foodItems = await query.OrderByDescending(i => i.Rating).ToListAsync();

            var activeItem = foodItems.FirstOrDefault() ?? await _context.FoodItems.Include(i => i.Category).FirstOrDefaultAsync();

            var recommended = activeItem != null 
                ? await _context.FoodItems.Where(i => i.Id != activeItem.Id).Take(4).ToListAsync()
                : new List<TblFoodItem>();

            var ingredients = (activeItem?.MainIngredients ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var viewModel = new FoodDetailViewModel
            {
                FoodItem = activeItem ?? new TblFoodItem { Title = "Menu Item", Description = "Explore our fine dining dishes", Price = 0 },
                Categories = categories,
                RecommendedItems = recommended,
                ParsedIngredients = ingredients,
                SelectedCategoryName = categoryId.HasValue ? categories.FirstOrDefault(c => c.Id == categoryId.Value)?.Name ?? "All" : "All"
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var activeItem = await _context.FoodItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (activeItem == null)
            {
                return NotFound("Dish not found.");
            }

            var categories = await _context.FoodCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var recommended = await _context.FoodItems
                .Where(i => i.Id != activeItem.Id && i.CategoryId == activeItem.CategoryId)
                .Take(4)
                .ToListAsync();

            if (!recommended.Any())
            {
                recommended = await _context.FoodItems
                    .Where(i => i.Id != activeItem.Id)
                    .Take(4)
                    .ToListAsync();
            }

            var ingredients = (activeItem.MainIngredients ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var viewModel = new FoodDetailViewModel
            {
                FoodItem = activeItem,
                Categories = categories,
                RecommendedItems = recommended,
                ParsedIngredients = ingredients,
                SelectedCategoryName = activeItem.Category?.Name ?? "All"
            };

            return View("Index", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetItemDetailJson(Guid id)
        {
            var item = await _context.FoodItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            var ingredients = (item.MainIngredients ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            return Json(new
            {
                id = item.Id,
                title = item.Title,
                description = item.Description,
                price = item.Price,
                rating = item.Rating,
                prepTime = item.EstimatedPrepTimeMinutes,
                imageUrl = item.ImageUrl,
                categoryName = item.Category?.Name,
                ingredients = ingredients
            });
        }
    }
}

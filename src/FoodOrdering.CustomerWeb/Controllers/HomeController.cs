using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FoodOrdering.Database;
using FoodOrdering.CustomerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.CustomerWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.FoodCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var foodItems = await _context.FoodItems
                .Include(i => i.Category)
                .OrderByDescending(i => i.Rating)
                .ToListAsync();

            var tables = await _context.Tables
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            ViewData["Categories"] = categories;
            ViewData["FoodItems"] = foodItems;
            ViewData["Tables"] = tables;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid? id, Guid? categoryId = null)
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

            var foodItems = await query.OrderByDescending(i => i.Rating).ToListAsync();

            var activeItem = id.HasValue
                ? await _context.FoodItems.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id.Value)
                : foodItems.FirstOrDefault() ?? await _context.FoodItems.Include(i => i.Category).FirstOrDefaultAsync();

            if (activeItem == null)
            {
                return NotFound("No food items found.");
            }

            var recommended = await _context.FoodItems
                .Where(i => i.Id != activeItem.Id)
                .Take(4)
                .ToListAsync();

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
                SelectedCategoryName = categoryId.HasValue ? categories.FirstOrDefault(c => c.Id == categoryId.Value)?.Name ?? "All" : "All"
            };

            return View("~/Views/Catalog/Index.cshtml", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

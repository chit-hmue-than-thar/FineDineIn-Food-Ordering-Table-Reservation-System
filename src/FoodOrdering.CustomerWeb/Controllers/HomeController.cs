using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FoodOrdering.Database;
using FoodOrdering.Domain.Entities;
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
            if (!await _context.FoodCategories.AnyAsync() || !await _context.FoodItems.AnyAsync())
            {
                await DbSeeder.SeedAsync(_context);
            }

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
            if (!await _context.FoodCategories.AnyAsync() || !await _context.FoodItems.AnyAsync())
            {
                await DbSeeder.SeedAsync(_context);
            }

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

            var targetCatName = categoryId.HasValue
                ? categories.FirstOrDefault(c => c.Id == categoryId.Value)?.Name ?? "Main Course"
                : activeItem?.Category?.Name ?? "Main Course";

            activeItem = EnsureValidFoodItem(activeItem, targetCatName);

            var recommended = await _context.FoodItems
                .Where(i => i.Id != activeItem.Id)
                .Take(4)
                .ToListAsync();

            if (recommended.Count < 3)
            {
                recommended = GetFallbackRecommendedItems();
            }

            var ingredients = (activeItem.MainIngredients ?? "Herb Butter, Garlic Sauce, Parmesan")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var viewModel = new FoodDetailViewModel
            {
                FoodItem = activeItem,
                Categories = categories,
                RecommendedItems = recommended,
                ParsedIngredients = ingredients,
                SelectedCategoryName = targetCatName
            };

            return View("~/Views/Catalog/Index.cshtml", viewModel);
        }

        private TblFoodItem EnsureValidFoodItem(TblFoodItem? item, string categoryName)
        {
            if (item != null && item.Price > 0 && !string.IsNullOrWhiteSpace(item.Title) && item.Title != "Menu Item")
            {
                return item;
            }

            return categoryName switch
            {
                "Starters" => new TblFoodItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Wagyu Beef Carpaccio",
                    Description = "Thinly sliced raw Wagyu beef with wild arugula, capers, aged parmesan, and truffle olive oil.",
                    Price = 25000m,
                    EstimatedPrepTimeMinutes = 12,
                    Rating = 4.8,
                    MainIngredients = "Aged Wagyu, Parmesan Shavings, Wild Arugula, Extra Virgin Olive Oil, Capers",
                    ImageUrl = "https://images.unsplash.com/photo-1541529086526-db283c563270?w=600",
                    Category = new TblFoodCategory { Name = "Starters" }
                },
                "Desserts" => new TblFoodItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Valrhona Chocolate Lava Cake",
                    Description = "Warm molten chocolate cake made with 70% dark cocoa, served with artisanal vanilla bean ice cream.",
                    Price = 18000m,
                    EstimatedPrepTimeMinutes = 15,
                    Rating = 4.9,
                    MainIngredients = "70% Valrhona Cocoa, Madagascar Vanilla Bean, Fresh Mint, Berry Sauce",
                    ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600",
                    Category = new TblFoodCategory { Name = "Desserts" }
                },
                "Beverages" => new TblFoodItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Smoked Hibiscus Sparkler",
                    Description = "Artisanal mocktail infusion of wild hibiscus, passion fruit puree, mint, and sparkling tonic.",
                    Price = 12000m,
                    EstimatedPrepTimeMinutes = 5,
                    Rating = 4.7,
                    MainIngredients = "Wild Hibiscus, Passion Fruit, Fresh Mint, Sparkling Water",
                    ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=600",
                    Category = new TblFoodCategory { Name = "Beverages" }
                },
                "Pizza" => new TblFoodItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Truffle & Mushroom Artisanal Pizza",
                    Description = "Wood-fired sourdough pizza topped with black truffle cream, wild forest mushrooms, and fresh mozzarella.",
                    Price = 32000m,
                    EstimatedPrepTimeMinutes = 18,
                    Rating = 4.8,
                    MainIngredients = "Sourdough Base, Black Truffle Cream, Forest Mushrooms, Fresh Mozzarella",
                    ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600",
                    Category = new TblFoodCategory { Name = "Pizza" }
                },
                "Chef Specials" => new TblFoodItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Pan-Seared Duck Breast",
                    Description = "Crispy skin duck breast served with dark cherry reduction, caramelized baby carrots, and potato fondant.",
                    Price = 65000m,
                    EstimatedPrepTimeMinutes = 25,
                    Rating = 4.9,
                    MainIngredients = "Prime Duck Breast, Dark Cherry Reduction, Baby Carrots, Potato Fondant",
                    ImageUrl = "https://images.unsplash.com/photo-1534422298391-e4f8c172dddb?w=600",
                    Category = new TblFoodCategory { Name = "Chef Specials" }
                },
                _ => new TblFoodItem
                {
                    Id = Guid.NewGuid(),
                    Title = "Prime Angus Ribeye Steak",
                    Description = "12oz pan-seared prime Angus ribeye steak served with garlic herb butter, truffle mash, and red wine jus.",
                    Price = 55000m,
                    EstimatedPrepTimeMinutes = 20,
                    Rating = 4.9,
                    MainIngredients = "Prime Ribeye, Garlic Herb Butter, Truffle Mash, Red Wine Jus",
                    ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=600",
                    Category = new TblFoodCategory { Name = "Main Course" }
                }
            };
        }

        private List<TblFoodItem> GetFallbackRecommendedItems()
        {
            return new List<TblFoodItem>
            {
                new TblFoodItem { Id = Guid.NewGuid(), Title = "Grilled Salmon", Price = 45000m, ImageUrl = "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=400", Category = new TblFoodCategory { Name = "Main Course" } },
                new TblFoodItem { Id = Guid.NewGuid(), Title = "Creamy Prawn Pasta", Price = 38000m, ImageUrl = "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=400", Category = new TblFoodCategory { Name = "Main Course" } },
                new TblFoodItem { Id = Guid.NewGuid(), Title = "Classic Tiramisu", Price = 15000m, ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400", Category = new TblFoodCategory { Name = "Desserts" } }
            };
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

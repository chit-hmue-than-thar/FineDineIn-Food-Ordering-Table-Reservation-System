using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodOrdering.Domain.Entities;
using FoodOrdering.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.Database
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            var now = DateTime.UtcNow.AddHours(6).AddMinutes(30);

            // 1. Seed Accounts if missing
            if (!await context.Users.AnyAsync())
            {
                var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

                var customer = new TblUser
                {
                    Id = Guid.NewGuid(),
                    Name = "Julian Vance",
                    Email = "customer@finedinein.com",
                    Phone = "+95 9 791 234 567",
                    PasswordHash = defaultPasswordHash,
                    Role = UserRole.Customer,
                    CreatedAt = now
                };

                var admin = new TblUser
                {
                    Id = Guid.NewGuid(),
                    Name = "Executive Chef Admin",
                    Email = "admin@finedinein.com",
                    Phone = "+95 9 799 999 999",
                    PasswordHash = defaultPasswordHash,
                    Role = UserRole.Admin,
                    CreatedAt = now
                };

                await context.Users.AddRangeAsync(customer, admin);
                await context.SaveChangesAsync();
            }

            // 2. Seed Categories matching reference UI if missing
            if (!await context.FoodCategories.AnyAsync())
            {
                var categories = new List<TblFoodCategory>
                {
                    new TblFoodCategory { Id = Guid.NewGuid(), Name = "Starters", Description = "Appetizing starters & soups", ImageUrl = "https://images.unsplash.com/photo-1541529086526-db283c563270?w=400", DisplayOrder = 1, CreatedAt = now },
                    new TblFoodCategory { Id = Guid.NewGuid(), Name = "Main Course", Description = "Masterfully grilled steaks & pasta", ImageUrl = "https://images.unsplash.com/photo-1547592180-85f173990554?w=400", DisplayOrder = 2, CreatedAt = now },
                    new TblFoodCategory { Id = Guid.NewGuid(), Name = "Desserts", Description = "Decadent plated cakes & soufflés", ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=400", DisplayOrder = 3, CreatedAt = now },
                    new TblFoodCategory { Id = Guid.NewGuid(), Name = "Beverages", Description = "Artisanal cocktails & elixirs", ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=400", DisplayOrder = 4, CreatedAt = now },
                    new TblFoodCategory { Id = Guid.NewGuid(), Name = "Pizza", Description = "Wood-fired sourdough artisanal pizzas", ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=400", DisplayOrder = 5, CreatedAt = now },
                    new TblFoodCategory { Id = Guid.NewGuid(), Name = "Chef Specials", Description = "Executive chef signature creations", ImageUrl = "https://images.unsplash.com/photo-1534422298391-e4f8c172dddb?w=400", DisplayOrder = 6, CreatedAt = now }
                };

                await context.FoodCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 3. Seed Items matching reference UI if missing
            if (!await context.FoodItems.AnyAsync())
            {
                var mainCategory = await context.FoodCategories.FirstOrDefaultAsync(c => c.Name == "Main Course") ?? await context.FoodCategories.FirstAsync();
                var dessertCategory = await context.FoodCategories.FirstOrDefaultAsync(c => c.Name == "Desserts") ?? mainCategory;
                var beverageCategory = await context.FoodCategories.FirstOrDefaultAsync(c => c.Name == "Beverages") ?? mainCategory;
                var pizzaCategory = await context.FoodCategories.FirstOrDefaultAsync(c => c.Name == "Pizza") ?? mainCategory;

                var items = new List<TblFoodItem>
                {
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = mainCategory.Id, Title = "Grilled Salmon",
                        Description = "Served with lemon butter sauce", Price = 24.99m,
                        EstimatedPrepTimeMinutes = 20, Rating = 4.8, MainIngredients = "Fresh Salmon, Lemon, Butter, Parsley",
                        ImageUrl = "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=500", CreatedAt = now
                    },
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = mainCategory.Id, Title = "Creamy Prawn Pasta",
                        Description = "Penne in creamy garlic sauce with fresh herbs", Price = 21.99m,
                        EstimatedPrepTimeMinutes = 18, Rating = 4.7, MainIngredients = "Tiger Prawns, Penne, Garlic, Cream, Tomato, Cheese",
                        ImageUrl = "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=500", CreatedAt = now
                    },
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = mainCategory.Id, Title = "Ribeye Steak",
                        Description = "Grilled to perfection", Price = 29.99m,
                        EstimatedPrepTimeMinutes = 25, Rating = 4.9, MainIngredients = "Prime Beef, Rosemary, Herb Butter",
                        ImageUrl = "https://images.unsplash.com/photo-1544025162-d76694265947?w=500", CreatedAt = now
                    },
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = dessertCategory.Id, Title = "Classic Tiramisu",
                        Description = "With cocoa & mascarpone", Price = 8.99m,
                        EstimatedPrepTimeMinutes = 10, Rating = 4.6, MainIngredients = "Espresso, Mascarpone, Cocoa",
                        ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=500", CreatedAt = now
                    },
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = mainCategory.Id, Title = "Truffle Mushroom Risotto",
                        Description = "Creamy arborio rice", Price = 18.99m,
                        EstimatedPrepTimeMinutes = 20, Rating = 4.9, MainIngredients = "Black Truffle, Arborio Rice, Parmesan",
                        ImageUrl = "https://images.unsplash.com/photo-1633964913295-ceb43826e7c9?w=500", CreatedAt = now
                    },
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = pizzaCategory.Id, Title = "Margherita Pizza",
                        Description = "Fresh basil & mozzarella", Price = 16.99m,
                        EstimatedPrepTimeMinutes = 15, Rating = 4.7, MainIngredients = "Fresh Mozzarella, Basil, Tomato",
                        ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=500", CreatedAt = now
                    },
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = beverageCategory.Id, Title = "Mango Passion Mocktail",
                        Description = "Tropical refreshing drink", Price = 7.99m,
                        EstimatedPrepTimeMinutes = 5, Rating = 4.8, MainIngredients = "Mango Puree, Passionfruit, Mint",
                        ImageUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=500", CreatedAt = now
                    },
                    new TblFoodItem
                    {
                        Id = Guid.NewGuid(), CategoryId = dessertCategory.Id, Title = "Chocolate Lava Cake",
                        Description = "Served with vanilla ice cream", Price = 9.99m,
                        EstimatedPrepTimeMinutes = 15, Rating = 4.9, MainIngredients = "Dark Chocolate, Vanilla Bean",
                        ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=500", CreatedAt = now
                    }
                };

                await context.FoodItems.AddRangeAsync(items);
                await context.SaveChangesAsync();
            }

            // 4. Seed Tables if missing
            if (!await context.Tables.AnyAsync())
            {
                var tables = new List<TblTable>
                {
                    new TblTable { Id = Guid.NewGuid(), TableNumber = "T-01", Capacity = 2, LocationType = TableLocationType.Indoor, Status = TableStatus.Available, Description = "Romantic indoor table near wine cellar.", QrCodeUrl = "/qr/T-01", CreatedAt = now },
                    new TblTable { Id = Guid.NewGuid(), TableNumber = "T-02", Capacity = 2, LocationType = TableLocationType.Indoor, Status = TableStatus.Available, Description = "Window table with city views.", QrCodeUrl = "/qr/T-02", CreatedAt = now },
                    new TblTable { Id = Guid.NewGuid(), TableNumber = "T-03", Capacity = 4, LocationType = TableLocationType.VIPBooth, Status = TableStatus.Available, Description = "Plush VIP booth with warm lighting.", QrCodeUrl = "/qr/T-03", CreatedAt = now },
                    new TblTable { Id = Guid.NewGuid(), TableNumber = "T-04", Capacity = 6, LocationType = TableLocationType.Terrace, Status = TableStatus.Available, Description = "Outdoor garden terrace setting.", QrCodeUrl = "/qr/T-04", CreatedAt = now }
                };

                await context.Tables.AddRangeAsync(tables);
                await context.SaveChangesAsync();
            }
        }
    }
}




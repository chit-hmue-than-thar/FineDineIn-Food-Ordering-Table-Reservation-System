using System;
using System.Collections.Generic;
using FoodOrdering.Domain.Entities;

namespace FoodOrdering.CustomerWeb.Models
{
    public class FoodDetailViewModel
    {
        public TblFoodItem FoodItem { get; set; } = new TblFoodItem();
        public List<TblFoodCategory> Categories { get; set; } = new List<TblFoodCategory>();
        public List<TblFoodItem> RecommendedItems { get; set; } = new List<TblFoodItem>();
        public List<string> ParsedIngredients { get; set; } = new List<string>();
        public string SelectedCategoryName { get; set; } = "All";
    }

    public class CartItemViewModel
    {
        public Guid FoodItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string PortionSize { get; set; } = "Standard";
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Total => Price * Quantity;
    }
}

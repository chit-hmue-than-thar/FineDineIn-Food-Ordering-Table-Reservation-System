using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FoodOrdering.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrdering.CustomerWeb.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound("User profile not found.");
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string name, string phone)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
            {
                TempData["ErrorMessage"] = "Name and Phone number are required.";
                return RedirectToAction("Index");
            }

            var (success, message) = await _userService.UpdateUserProfileAsync(userId, name, phone);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction("Index");
        }
    }
}

using AriaPrint.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AriaPrint.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        [Route("/Profile")]
        public IActionResult Profile()
        {
            var firstName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? "کاربر";
            var lastName = User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value ?? "";
            var phone = User.Identity?.Name ?? "—";
            var fullName = (firstName + " " + lastName).Trim();
            if (string.IsNullOrWhiteSpace(fullName)) fullName = "کاربر";
            var isAdmin = User.IsInRole("Admin");
            var initial = string.IsNullOrWhiteSpace(firstName) ? "?" : firstName.Substring(0, 1);

            var ViewModel = new ProfileViewModel
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                FullName = fullName,
                IsAdmin = isAdmin,
                Initial = initial,
            };

            return View(ViewModel);
        }
    }
}

using AriaPrint.BL;
using AriaPrint.Models;
using AriaPrint.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AriaPrint.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepo;
        private readonly PasswordService _passwordService;

        public AccountController(UserRepository userRepo, PasswordService passwordService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (!long.TryParse(model.PhoneNumber.Trim(), out long phoneNumber))
            {
                ModelState.AddModelError("", "فرمت شماره موبایل اشتباه است.");
                return View(model);
            }

            var existing = await _userRepo.GetByPhoneNumberAsync(phoneNumber);
            if (existing != null)
            {
                ModelState.AddModelError("", "این شماره موبایل قبلاً ثبت شده است.");
                return View(model);
            }

            var user = new AppUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = phoneNumber,
                PasswordHash = _passwordService.HashPassword(model.Password),
                Role = "User"
            };

            var newId = await _userRepo.CreateAsync(user);
            user.Id = newId;

            await SignInUserAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!long.TryParse(model.PhoneNumber.Trim(), out long phoneNumber))
            {
                ModelState.AddModelError("", "فرمت شماره موبایل اشتباه است.");
                return View(model);
            }

            var user = await _userRepo.GetByPhoneNumberAsync(phoneNumber);
            if (user == null || !_passwordService.VerifyPassword(user.PasswordHash, model.Password))
            {
                ModelState.AddModelError("", "شماره موبایل یا رمز عبور اشتباه است.");
                return View(model);
            }

            await SignInUserAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => Redirect("/Account/Login?mode=forgot");

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            return Redirect("/Account/Login?mode=forgot-sent");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private async Task SignInUserAsync(AppUser user)
        {
            var claims = new List<Claim>
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role)
            };

            //var claims = new List<Claim>
            //{
            //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //new Claim(ClaimTypes.Name, user.PhoneNumber.ToString()),
            //new Claim(ClaimTypes.GivenName, user.FirstName),
            //new Claim(ClaimTypes.Surname, user.LastName),
            //new Claim(ClaimTypes.Role, user.Role)
            //};

            var scheme = CookieAuthenticationDefaults.AuthenticationScheme;
            var identity = new ClaimsIdentity(claims, scheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                scheme,
                principal,
                new AuthenticationProperties { IsPersistent = true }
            );
        }
    }
}

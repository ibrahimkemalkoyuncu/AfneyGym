using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using AfneyGym.Common.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AfneyGym.Common.Enums;

namespace AfneyGym.WebMvc.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        // 1. DTO Validation kontrolü
        if (!ModelState.IsValid) return View(loginDto);

        // 2. Servis üzerinden BCrypt doğrulamalı giriş
        var user = await _userService.LoginAsync(loginDto);

        if (user != null)
        {
            // 3. Claims oluşturma (Identity tabanlı yetkilendirme için)
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Admin, Staff veya Member
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = true };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // 4. Role göre yönlendirme (Mühendislik kararı: Admin paneli veya Ana sayfa)
            if (user.Role == UserRole.Admin)
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
        return View(loginDto);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserRegisterDto registerDto)
    {
        if (!ModelState.IsValid) return View(registerDto);

        var result = await _userService.RegisterAsync(registerDto);

        if (result)
        {
            TempData["SuccessMessage"] = "Üyeliğiniz başarıyla oluşturuldu. Giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError("Email", "bu e-posta adresi zaten kullanımda.");
        return View(registerDto);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
/* AÇIKLAMA: 
   - Login ve Register metodları asenkron (Task) yapıya taşındı.
   - User entity'si yerine DTO'lar kullanılarak veri sızıntısı önlendi.
   - SignInAsync ile Cookie tabanlı oturum yönetimi tescillendi.
*/
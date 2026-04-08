using AfneyGym.Common.DTOs;
using AfneyGym.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AfneyGym.WebMvc.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public AccountController(IUserService userService, IEmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
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
            try
            {
                await _emailService.SendEmailAsync(registerDto.Email, "AfneyGym'e Hoşgeldiniz!", "Kaydınız başarıyla tamamlandı.");
            }
            catch { /* Loglama */ }

            TempData["SuccessMessage"] = "Kaydınız başarıyla tamamlandı. Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
        return View(registerDto);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        if (!ModelState.IsValid) return View(loginDto);

        var user = await _userService.LoginAsync(loginDto);
        if (user != null)
        {
            // MÜHENDİSLİK DÜZELTMESİ: Rol ismini açıkça string olarak (Admin, Member) alıyoruz.
            // Enum değeri 0 ise "Admin" olarak dönecektir.
            string userRole = user.Role.ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, userRole), // Burası "Admin" olmalı
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }); // Beni hatırla özelliği eklendi

            // Eğer Admin ise doğrudan Dashboard'a yönlendir
            if (userRole == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
        return View(loginDto);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
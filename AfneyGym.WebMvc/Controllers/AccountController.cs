using AfneyGym.Common.DTOs;
using AfneyGym.Common.Enums;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using AfneyGym.Service.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AfneyGym.WebMvc.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context; // Profil detayları için eklendi
    public AccountController(IUserService userService, IEmailService emailService, AppDbContext context)
    {
        _userService = userService;
        _emailService = emailService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Login() => View();

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Login(UserLoginDto loginDto)
    //{
    //    // 1. DTO Validation kontrolü
    //    if (!ModelState.IsValid) return View(loginDto);

    //    // 2. Servis üzerinden BCrypt doğrulamalı giriş
    //    var user = await _userService.LoginAsync(loginDto);

    //    if (user != null)
    //    {
    //        // 3. Claims oluşturma (Identity tabanlı yetkilendirme için)
    //        var claims = new List<Claim>
    //        {
    //            new Claim("UserId", user.Id.ToString()),
    //            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
    //            new Claim(ClaimTypes.Email, user.Email),
    //            new Claim(ClaimTypes.Role, user.Role.ToString()) // Admin, Staff veya Member
    //        };

    //        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    //        var authProperties = new AuthenticationProperties { IsPersistent = true };

    //        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
    //            new ClaimsPrincipal(claimsIdentity), authProperties);

    //        // 4. Role göre yönlendirme (Mühendislik kararı: Admin paneli veya Ana sayfa)
    //        if (user.Role == UserRole.Admin)
    //            return RedirectToAction("Dashboard", "Admin");

    //        return RedirectToAction("Index", "Home");
    //    }

    //    ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
    //    return View(loginDto);
    //}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        if (!ModelState.IsValid) return View(loginDto);

        var user = await _userService.LoginAsync(loginDto);
        if (user == null)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(loginDto);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("UserId", user.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return user.Role == UserRole.Admin
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Register(UserRegisterDto registerDto)
    //{
    //    if (!ModelState.IsValid) return View(registerDto);

    //    var result = await _userService.RegisterAsync(registerDto);
    //    if (result)
    //    {
    //        // REGRESSION CHECK: Metod ismi interface ile uyumlu hale getirildi
    //        //await _emailService.SendEmailAsync(registerDto.Email, "Hoş Geldiniz", "AfneyGym ailesine katıldığınız için teşekkürler!");

    //        return RedirectToAction(nameof(Login));
    //    }

    //    ModelState.AddModelError("", "Bu e-posta adresi zaten kullanımda.");
    //    return View(registerDto);
    //}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserRegisterDto registerDto)
    {
        if (!ModelState.IsValid) return View(registerDto);

        var result = await _userService.RegisterAsync(registerDto);
        if (result)
        {
            // Regresyon Kontrolü: Servis metod ismi senkronize edildi.
            await _emailService.SendEmailAsync(registerDto.Email, "Hoş Geldiniz", "Kaydınız başarıyla tamamlandı.");
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError("", "Bu e-posta adresi zaten kullanımda.");
        return View(registerDto);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    //[Authorize]
    //public async Task<IActionResult> Profile()
    //{
    //    var userIdClaim = User.FindFirst("UserId")?.Value;
    //    if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction(nameof(Login));

    //    var userId = Guid.Parse(userIdClaim);

    //    // MÜHENDİSLİK ANALİZİ: Profil verisini tüm ilişkileriyle (Gym, Attendees, Lessons) tek seferde çekiyoruz.
    //    var user = await _context.Users
    //        .Include(u => u.Gym)
    //        .Include(u => u.Subscriptions)
    //        .Include(u => u.Attendees).ThenInclude(a => a.Lesson).ThenInclude(l => l.Trainer)
    //        .FirstOrDefaultAsync(u => u.Id == userId);

    //    if (user == null) return NotFound();

    //    var profileDto = new UserProfileDto
    //    {
    //        FullName = $"{user.FirstName} {user.LastName}",
    //        Email = user.Email,
    //        GymName = user.Gym?.Name ?? "Şube Atanmamış",
    //        TotalAttendedLessons = user.Attendees.Count,
    //        SubscriptionEndDate = user.Subscriptions.OrderByDescending(s => s.EndDate).FirstOrDefault()?.EndDate,
    //        AttendedLessons = user.Attendees.OrderByDescending(a => a.Lesson.StartTime).Select(a => new UserLessonHistoryDto
    //        {
    //            LessonName = a.Lesson.Name,
    //            TrainerName = a.Lesson.Trainer?.FullName ?? "Belirtilmemiş",
    //            LessonDate = a.Lesson.StartTime,
    //            TimeSlot = $"{a.Lesson.StartTime:HH:mm} - {a.Lesson.EndTime:HH:mm}"
    //        }).ToList()
    //    };

    //    return View(profileDto);
    //}

    #region Profil Yönetimi
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        // UserId Claim üzerinden güvenli erişim
        var userIdStr = User.FindFirst("UserId")?.Value;
        if (!Guid.TryParse(userIdStr, out var userId)) return RedirectToAction(nameof(Login));

        // PROJECTION: Karmaşık ilişkisel veri tek sorguda çekilir (Eager Loading)
        var user = await _context.Users
            .Include(u => u.Gym)
            .Include(u => u.Subscriptions)
            .Include(u => u.Attendees!)
                .ThenInclude(a => a.Lesson!)
                .ThenInclude(l => l.Trainer)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null) return NotFound();

        // DTO Mapping (View katmanındaki property isimleriyle tam uyum)
        var profileDto = new UserProfileDto
        {
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            GymName = user.Gym?.Name ?? "Şube Atanmamış",
            TotalAttendedLessons = user.Attendees?.Count ?? 0,
            SubscriptionEndDate = user.Subscriptions?.OrderByDescending(s => s.EndDate).FirstOrDefault()?.EndDate,
            AttendedLessons = user.Attendees?
                .OrderByDescending(a => a.Lesson!.StartTime)
                .Select(a => new UserLessonHistoryDto
                {
                    LessonName = a.Lesson!.Name,
                    TrainerName = a.Lesson.Trainer?.FullName ?? "Belirtilmemiş",
                    LessonDate = a.Lesson.StartTime,
                    TimeSlot = $"{a.Lesson.StartTime:HH:mm} - {a.Lesson.EndTime:HH:mm}"
                }).ToList() ?? new List<UserLessonHistoryDto>()
        };

        return View(profileDto);
    }
    #endregion
}
/* AÇIKLAMA: 
   - Login ve Register metodları asenkron (Task) yapıya taşındı.
   - User entity'si yerine DTO'lar kullanılarak veri sızıntısı önlendi.
   - SignInAsync ile Cookie tabanlı oturum yönetimi tescillendi.
*/
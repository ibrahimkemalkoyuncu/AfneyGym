using AfneyGym.Common.DTOs;
using AfneyGym.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfneyGym.WebMvc.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AfneyGym.WebMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILessonService _lessonService;
    private readonly IDashboardService _dashboardService;
    private readonly IMemoryCache _cache;

    public HomeController(ILessonService lessonService, IDashboardService dashboardService, IMemoryCache cache)
    {
        _lessonService = lessonService;
        _dashboardService = dashboardService;
        _cache = cache;
    }

    //public async Task<IActionResult> Index()
    //{
    //    var lessons = await _context.Lessons
    //        .Include(l => l.Trainer)
    //        .Include(l => l.Attendees)
    //        .Where(l => !l.IsDeleted)
    //        .OrderBy(l => l.StartTime)
    //        .ToListAsync();

    //    return View(lessons);
    //}

    public async Task<IActionResult> Index(string? hero)
    {
        var variant = Request.Cookies.TryGetValue("HeroVariant", out var savedVariant)
            ? NormalizeVariant(savedVariant)
            : NormalizeVariant(hero);

        // Varyantı cookie'ye yaz (30 gün)
        Response.Cookies.Append("HeroVariant", variant, new CookieOptions
        {
            MaxAge = TimeSpan.FromDays(30),
            HttpOnly = true,
            IsEssential = true
        });

        var visitorId = GetOrCreateVisitorId();
        await _dashboardService.TrackHeroVariantExposureAsync(visitorId, variant);

        LandingKpiDto kpis;

        // Fallback: cache'den al yoksa sorgu yap ve cache'le (5 dakika)
        if (!_cache.TryGetValue("LandingKpis", out LandingKpiDto? cachedKpis) || cachedKpis is null)
        {
            kpis = await _dashboardService.GetLandingKpisAsync();
            _cache.Set("LandingKpis", kpis, TimeSpan.FromMinutes(5));
        }
        else
        {
            kpis = cachedKpis;
        }

        var vm = new HomeIndexViewModel
        {
            HeroVariant = variant,
            Kpis = kpis
        };

        return View(vm);
    }

    [HttpGet("/classes")]
    public async Task<IActionResult> Lessons()
    {
        var lessons = await _lessonService.GetAllWithTrainersAsync();
        return View("Classes", lessons);
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinLesson(Guid lessonId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Account");
        var userId = Guid.Parse(userIdClaim);

        var joinResult = await _lessonService.JoinLessonAsync(lessonId, userId);

        switch (joinResult)
        {
            case JoinLessonStatus.Success:
                TempData["SuccessMessage"] = "Derse başarıyla kayıt oldunuz!";
                break;
            case JoinLessonStatus.NoActiveSubscription:
                TempData["ErrorMessage"] = "Bu derse katılmak için aktif bir üyeliğiniz olmalıdır.";
                break;
            case JoinLessonStatus.CapacityFull:
                TempData["ErrorMessage"] = "Üzgünüz, bu dersin kontenjanı doldu.";
                break;
            case JoinLessonStatus.AlreadyJoined:
                TempData["ErrorMessage"] = "Bu derse zaten kayıtlısınız.";
                break;
            case JoinLessonStatus.TimeConflict:
                TempData["ErrorMessage"] = "Sectiginiz dersin saati, zaten kayitli oldugunuz baska bir dersle cakisiyor.";
                break;
            case JoinLessonStatus.LessonNotFound:
                TempData["ErrorMessage"] = "Ders bulunamadı veya artık erişilebilir değil.";
                break;
            default:
                TempData["ErrorMessage"] = "Ders kaydı sırasında beklenmeyen bir hata oluştu.";
                break;
        }

        return RedirectToAction(nameof(Lessons));
    }

    // --- YENİ: DERS İPTAL METODU ---
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelJoin(Guid lessonId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Account");
        var userId = Guid.Parse(userIdClaim);

        var cancelResult = await _lessonService.CancelJoinAsync(lessonId, userId);
        switch (cancelResult)
        {
            case CancelJoinStatus.Success:
                TempData["SuccessMessage"] = "Ders katiliminiz iptal edildi.";
                break;
            case CancelJoinStatus.NotJoined:
                TempData["ErrorMessage"] = "Iptal edilecek bir kayit bulunamadi.";
                break;
            case CancelJoinStatus.LessonNotFound:
                TempData["ErrorMessage"] = "Ders bulunamadi veya artik erisilebilir degil.";
                break;
            default:
                TempData["ErrorMessage"] = "Ders iptali sirasinda beklenmeyen bir hata olustu.";
                break;
        }

        return RedirectToAction(nameof(Lessons));
    }

    [HttpGet("/design")]
    public IActionResult Components()
    {
        return View();
    }

    private string GetOrCreateVisitorId()
    {
        if (Request.Cookies.TryGetValue("HeroVisitorId", out var existingVisitorId) && !string.IsNullOrWhiteSpace(existingVisitorId))
            return existingVisitorId;

        var visitorId = Guid.NewGuid().ToString("N");
        Response.Cookies.Append("HeroVisitorId", visitorId, new CookieOptions
        {
            MaxAge = TimeSpan.FromDays(90),
            HttpOnly = true,
            IsEssential = true
        });

        return visitorId;
    }

    private static string NormalizeVariant(string? variant)
    {
        return string.Equals(variant, "b", StringComparison.OrdinalIgnoreCase) ? "b" : "a";
    }
}
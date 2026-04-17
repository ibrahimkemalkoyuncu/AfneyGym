using AfneyGym.Common.DTOs;
using AfneyGym.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfneyGym.WebMvc.Controllers;

[Authorize(Roles = "Member")]
public class MemberController : Controller
{
    private readonly IMemberLifecycleService _memberLifecycleService;

    public MemberController(IMemberLifecycleService memberLifecycleService)
    {
        _memberLifecycleService = memberLifecycleService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        var stage = await _memberLifecycleService.GetCurrentStageAsync(userId.Value);
        var monthlyCheckIn = await _memberLifecycleService.GetMonthlyCheckInCountAsync(userId.Value);
        var goals = await _memberLifecycleService.GetActiveGoalsAsync(userId.Value);
        var latestMetric = await _memberLifecycleService.GetLatestBodyMetricSummaryAsync(userId.Value);
        var checkIns = await _memberLifecycleService.GetRecentCheckInsAsync(userId.Value, 30);

        ViewBag.Stage = stage.ToString();
        ViewBag.MonthlyCheckIn = monthlyCheckIn;
        ViewBag.Goals = goals;
        ViewBag.LatestMetric = latestMetric;
        ViewBag.CheckIns = checkIns;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        var success = await _memberLifecycleService.CheckInAsync(userId.Value);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Check-in kaydiniz olusturuldu."
            : "Acik bir check-in kaydiniz olabilir.";

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        var success = await _memberLifecycleService.CheckOutAsync(userId.Value);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Check-out kaydiniz alindi."
            : "Acik check-in kaydi bulunamadi.";

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGoal(UserGoalCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(dto.Title) || dto.TargetValue == 0)
        {
            TempData["ErrorMessage"] = "Hedef bilgileri gecersiz.";
            return RedirectToAction(nameof(Dashboard));
        }

        await _memberLifecycleService.AddGoalAsync(userId.Value, dto);
        TempData["SuccessMessage"] = "Yeni hedefiniz kaydedildi.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBodyMetric(BodyMetricCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        if (dto.Weight <= 0)
        {
            TempData["ErrorMessage"] = "Kilo degeri 0'dan buyuk olmali.";
            return RedirectToAction(nameof(Dashboard));
        }

        await _memberLifecycleService.AddBodyMetricAsync(userId.Value, dto);
        TempData["SuccessMessage"] = "Olcum bilginiz kaydedildi.";
        return RedirectToAction(nameof(Dashboard));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdRaw = User.FindFirst("UserId")?.Value;
        return Guid.TryParse(userIdRaw, out var userId) ? userId : null;
    }
}


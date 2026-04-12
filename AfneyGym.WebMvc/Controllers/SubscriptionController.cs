using AfneyGym.Common.DTOs;
using AfneyGym.Common.Enums;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AfneyGym.WebMvc.Controllers;

[Authorize]
public class SubscriptionController : Controller
{
    // FIELD MANAGEMENT PROTOKOLÜ: Tekil ve Readonly Alan Tanımları
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public SubscriptionController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    #region Üye Aksiyonları (Member Actions)

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Paket listesini ve mevcut abonelik durumunu getir
        var userId = GetUserId();
        var currentSub = await _context.Subscriptions
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        ViewBag.CurrentSubscription = currentSub;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurchaseRequest(int months)
    {
        // MÜHENDİSLİK ANALİZİ: Bekleyen bir talep varsa yenisini engelle
        var userId = GetUserId();
        bool hasPendingRequest = await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Pending);

        if (hasPendingRequest)
        {
            TempData["ErrorMessage"] = "Zaten onay bekleyen bir abonelik talebiniz bulunmaktadır.";
            return RedirectToAction(nameof(Index));
        }

        var newSub = new Subscription
        {
            UserId = userId,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddMonths(months),
            Status = SubscriptionStatus.Pending,
            PlanName = $"{months} Aylık Paket", // Hata veren zorunlu alan
            CreatedAt = DateTime.Now                  
        };

        await _context.Subscriptions.AddAsync(newSub);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Abonelik talebiniz alındı. Ödeme onayından sonra aktif edilecektir.";
        return RedirectToAction("Profile", "Account");
    }
    #endregion

    #region Admin Aksiyonları (Admin Actions)

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageRequests()
    {
        var pendingRequests = await _context.Subscriptions
            .Include(s => s.User)
            .Where(s => s.Status == SubscriptionStatus.Pending && !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        return View(pendingRequests);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        var sub = await _context.Subscriptions.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        if (sub == null) return NotFound();

        sub.Status = SubscriptionStatus.Active;
        sub.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        // REGRESSION CHECK: Onay sonrası kullanıcıya bilgilendirme e-postası
        await _emailService.SendEmailAsync(sub.User.Email, "Aboneliğiniz Aktif Edildi", "Ödemeniz onaylandı. Artık derslere kayıt olabilirsiniz!");

        return RedirectToAction(nameof(ManageRequests));
    }
    #endregion

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        var sub = await _context.Subscriptions.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        if (sub == null) return NotFound();

        sub.Status = SubscriptionStatus.Rejected;
        sub.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        // Üyeye bilgilendirme maili gönder
        await _emailService.SendEmailAsync(sub.User!.Email, "Abonelik Talebi Hakkında",
            "Ödeme talebiniz onaylanamadı. Lütfen bilgilerinizi kontrol edip tekrar deneyiniz.");

        //await _emailService.SendEmailAsync(sub.User!.Email, "Abonelik Reddi", "Ödemeniz teyit edilemediği için talebiniz reddedildi.");

        TempData["ErrorMessage"] = "Talep reddedildi ve kullanıcı bilgilendirildi.";
        return RedirectToAction(nameof(ManageRequests));
    }

    #region DRY HELPERS
    private Guid GetUserId()
    {
        var userIdStr = User.FindFirstValue("UserId");
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }
    #endregion
}
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using AfneyGym.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using System.Text;

namespace AfneyGym.WebMvc.Controllers;

[Authorize]
public class SubscriptionController : Controller
{
    // FIELD MANAGEMENT PROTOKOLÜ: Tekil ve Readonly Alan Tanımları
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IIyzicoGateway _iyzicoGateway;
    private readonly SubscriptionRenewalService _subscriptionRenewalService;

    public SubscriptionController(AppDbContext context, IEmailService emailService, IWebHostEnvironment webHostEnvironment, IIyzicoGateway iyzicoGateway, SubscriptionRenewalService subscriptionRenewalService)
    {
        _context = context;
        _emailService = emailService;
        _webHostEnvironment = webHostEnvironment;
        _iyzicoGateway = iyzicoGateway;
        _subscriptionRenewalService = subscriptionRenewalService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    #region Üye Aksiyonları (Member Actions)

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Paket listesini ve mevcut abonelik durumunu getir
        var userId = GetUserId();
        await _subscriptionRenewalService.ProcessDueSubscriptionsAsync(userId);

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
            Price = GetPlanPrice(months),
            AutoRenew = false,
            CreatedAt = DateTime.Now
        };

        await _context.Subscriptions.AddAsync(newSub);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Abonelik talebiniz alındı. Ödeme onayından sonra aktif edilecektir.";
        return RedirectToAction("Profile", "Account");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartIyzicoCheckout(Guid subscriptionId)
    {
        var userId = GetUserId();
        var sub = await _context.Subscriptions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId && !s.IsDeleted);

        if (sub == null) return NotFound();

        var checkout = await _iyzicoGateway.CreateCheckoutFormUrlAsync(sub, sub.User!);
        if (!checkout.IsSuccess || string.IsNullOrWhiteSpace(checkout.CheckoutFormUrl))
        {
            TempData["ErrorMessage"] = $"iyzico odeme baslatilamadi. {checkout.ErrorMessage}";
            return RedirectToAction(nameof(Index));
        }

        _context.Payments.Add(new Payment
        {
            SubscriptionId = sub.Id,
            UserId = sub.UserId,
            Amount = sub.Price,
            Currency = "TRY",
            Provider = "Iyzico",
            ExternalReference = checkout.ConversationId,
            Status = PaymentStatus.Pending,
            Note = "iyzico checkout baslatildi"
        });

        await _context.SaveChangesAsync();
        return Redirect(checkout.CheckoutFormUrl);
    }

    [AcceptVerbs("GET", "POST")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> IyzicoCallback(string? token, string? conversationId)
    {
        var verifyResult = await _iyzicoGateway.VerifyCallbackAsync(token ?? string.Empty, conversationId);
        var resolvedConversationId = verifyResult.ConversationId;

        if (string.IsNullOrWhiteSpace(resolvedConversationId))
            resolvedConversationId = conversationId ?? string.Empty;

        var payment = await _context.Payments
            .Include(p => p.Subscription)
            .ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(p => p.ExternalReference == resolvedConversationId && !p.IsDeleted);

        if (payment == null)
            return BadRequest("Odeme kaydi bulunamadi.");

        if (!verifyResult.IsSuccess)
        {
            payment.Status = PaymentStatus.Failed;
            payment.Note = $"iyzico callback basarisiz: {verifyResult.ErrorMessage}";
            payment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "Odeme dogrulanamadi. Lutfen tekrar deneyin.";
            return RedirectToAction(nameof(Index));
        }

        if (payment.Status != PaymentStatus.Completed)
        {
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.Now;
            payment.ExternalPaymentId = verifyResult.PaymentId;
            payment.UpdatedAt = DateTime.Now;

            if (verifyResult.PaidPrice > 0)
                payment.Amount = verifyResult.PaidPrice;

            if (!string.IsNullOrWhiteSpace(verifyResult.Currency))
                payment.Currency = verifyResult.Currency;

            payment.Note = "iyzico callback onayi alindi";

            var sub = payment.Subscription;
            if (sub != null)
            {
                sub.Status = SubscriptionStatus.Active;
                sub.LastRenewalDate = DateTime.Now;
                sub.UpdatedAt = DateTime.Now;

                var hasInvoice = await _context.Invoices.AnyAsync(i => i.PaymentId == payment.Id && !i.IsDeleted);
                if (!hasInvoice && sub.User != null)
                {
                    var invoiceNumber = GenerateInvoiceNumber();
                    var htmlReceipt = BuildHtmlReceipt(sub, sub.User, payment, invoiceNumber);
                    var pdfRelativePath = SaveReceiptPdf(sub, sub.User, payment, invoiceNumber);

                    _context.Invoices.Add(new Invoice
                    {
                        PaymentId = payment.Id,
                        UserId = sub.UserId,
                        InvoiceNumber = invoiceNumber,
                        IssuedAt = DateTime.Now,
                        HtmlReceipt = htmlReceipt,
                        PdfRelativePath = pdfRelativePath,
                        EmailSent = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            var createdInvoice = await _context.Invoices
                .Where(i => i.PaymentId == payment.Id && !i.IsDeleted)
                .OrderByDescending(i => i.IssuedAt)
                .FirstOrDefaultAsync();

            if (createdInvoice != null && sub?.User != null)
            {
                await _emailService.SendEmailAsync(
                    sub.User.Email,
                    "iyzico odemeniz basariyla alindi",
                    $"{createdInvoice.HtmlReceipt}<p style='margin-top:16px;'>PDF makbuzunuz: <strong>{createdInvoice.InvoiceNumber}.pdf</strong></p>");

                createdInvoice.EmailSent = true;
                await _context.SaveChangesAsync();
            }
        }

        TempData["SuccessMessage"] = "iyzico odeme bildirimi basariyla isledi.";
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Admin Aksiyonları (Admin Actions)

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("/owner/approvals")]
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
    [Authorize(Roles = "Admin,Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id)
    {
        var sub = await _context.Subscriptions.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        if (sub == null) return NotFound();

        if (sub.Status != SubscriptionStatus.Pending)
        {
            TempData["ErrorMessage"] = "Bu talep zaten işlenmiş.";
            return RedirectToAction(nameof(ManageRequests));
        }

        sub.Status = SubscriptionStatus.Active;
        sub.UpdatedAt = DateTime.Now;
        sub.LastRenewalDate = DateTime.Now;

        var payment = new Payment
        {
            SubscriptionId = sub.Id,
            UserId = sub.UserId,
            Amount = sub.Price,
            Currency = "TRY",
            Provider = "Manual",
            ExternalReference = $"MANUAL-{sub.Id:N}-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.Now,
            Note = "Salon sahibi manuel ödeme onayı"
        };

        _context.Payments.Add(payment);

        var invoiceNumber = GenerateInvoiceNumber();
        var htmlReceipt = BuildHtmlReceipt(sub, sub.User!, payment, invoiceNumber);
        var pdfRelativePath = SaveReceiptPdf(sub, sub.User!, payment, invoiceNumber);

        var invoice = new Invoice
        {
            Payment = payment,
            UserId = sub.UserId,
            InvoiceNumber = invoiceNumber,
            IssuedAt = DateTime.Now,
            HtmlReceipt = htmlReceipt,
            PdfRelativePath = pdfRelativePath,
            EmailSent = false
        };

        _context.Invoices.Add(invoice);

        await _context.SaveChangesAsync();

        // REGRESSION CHECK: Onay sonrası kullanıcıya bilgilendirme e-postası
        await _emailService.SendEmailAsync(
            sub.User!.Email,
            "Aboneliğiniz Aktif Edildi",
            $"{htmlReceipt}<p style='margin-top:16px;'>PDF makbuzunuz sistemde oluşturuldu: <strong>{invoiceNumber}.pdf</strong></p>");

        invoice.EmailSent = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageRequests));
    }
    #endregion

    [HttpPost]
    [Authorize(Roles = "Admin,Owner")]
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Freeze(Guid id)
    {
        var userId = GetUserId();
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && !s.IsDeleted);
        if (sub == null) return NotFound();
        if (sub.Status != SubscriptionStatus.Active) return BadRequest("Sadece aktif üyelik dondurulabilir.");

        sub.IsFrozen = true;
        sub.FreezeDate = DateTime.Now;
        sub.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Üyeliğiniz donduruldu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resume(Guid id)
    {
        var userId = GetUserId();
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && !s.IsDeleted);
        if (sub == null) return NotFound();
        if (!sub.IsFrozen) return BadRequest("Üyelik zaten aktif durumda.");

        sub.IsFrozen = false;
        sub.FreezeDate = null;
        sub.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Üyeliğiniz yeniden aktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetUserId();
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && !s.IsDeleted);
        if (sub == null) return NotFound();

        sub.Status = SubscriptionStatus.Canceled;
        sub.CanceledAt = DateTime.Now;
        sub.AutoRenew = false;
        sub.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Üyeliğiniz iptal edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Renew(Guid id, int months = 1)
    {
        var userId = GetUserId();
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && !s.IsDeleted);
        if (sub == null) return NotFound();
        if (sub.Status == SubscriptionStatus.Canceled) return BadRequest("İptal edilmiş üyelik yenilenemez.");

        sub.EndDate = sub.EndDate > DateTime.Now ? sub.EndDate.AddMonths(months) : DateTime.Now.AddMonths(months);
        sub.Status = SubscriptionStatus.Active;
        sub.IsFrozen = false;
        sub.LastRenewalDate = DateTime.Now;
        sub.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Üyeliğiniz {months} ay uzatıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAutoRenew(Guid id)
    {
        var userId = GetUserId();
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId && !s.IsDeleted);
        if (sub == null) return NotFound();

        sub.AutoRenew = !sub.AutoRenew;
        sub.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = sub.AutoRenew
            ? "Otomatik yenileme aktif edildi."
            : "Otomatik yenileme kapatıldı.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpgradePlan(Guid id, int additionalMonths)
    {
        var sub = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        if (sub == null) return NotFound();
        if (sub.Status != SubscriptionStatus.Active) return BadRequest("Sadece aktif üyelik yükseltilebilir.");

        sub.EndDate = sub.EndDate.AddMonths(additionalMonths);
        sub.PlanName = $"{sub.PlanName} + {additionalMonths} Ay Upgrade";
        sub.Price += GetPlanPrice(additionalMonths);
        sub.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Üyelik planı yükseltildi.";
        return RedirectToAction("Members", "Admin");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DownloadInvoice(Guid invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

        if (invoice == null) return NotFound();

        var userId = GetUserId();
        var canAccess = invoice.UserId == userId || User.IsInRole("Admin") || User.IsInRole("Owner");
        if (!canAccess) return Forbid();

        var pdfPath = Path.Combine(_webHostEnvironment.WebRootPath, invoice.PdfRelativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(pdfPath)) return NotFound();

        return PhysicalFile(pdfPath, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
    }

    #region DRY HELPERS
    private Guid GetUserId()
    {
        var userIdStr = User.FindFirstValue("UserId");
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }

    private static decimal GetPlanPrice(int months)
    {
        return months switch
        {
            <= 0 => 0,
            1 => 1200m,
            3 => 3300m,
            6 => 6000m,
            12 => 10800m,
            _ => months * 1150m
        };
    }

    private static string GenerateInvoiceNumber() => $"INV-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";

    private static string BuildHtmlReceipt(Subscription subscription, User user, Payment payment, string invoiceNumber)
    {
        var sb = new StringBuilder();
        sb.Append("<div style='font-family:Arial,sans-serif;color:#111;'>");
        sb.Append($"<h2>AfneyGym Makbuz - {invoiceNumber}</h2>");
        sb.Append($"<p><strong>Uye:</strong> {user.FirstName} {user.LastName}</p>");
        sb.Append($"<p><strong>E-posta:</strong> {user.Email}</p>");
        sb.Append($"<p><strong>Paket:</strong> {subscription.PlanName}</p>");
        sb.Append($"<p><strong>Donem:</strong> {subscription.StartDate:dd.MM.yyyy} - {subscription.EndDate:dd.MM.yyyy}</p>");
        sb.Append($"<p><strong>Tutar:</strong> {payment.Amount:N2} {payment.Currency}</p>");
        sb.Append($"<p><strong>Odeme Yontemi:</strong> {payment.Provider}</p>");
        sb.Append($"<p><strong>Tarih:</strong> {payment.PaidAt:dd.MM.yyyy HH:mm}</p>");
        sb.Append("<p>Bu belge dijital olarak olusturulmustur.</p>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private string SaveReceiptPdf(Subscription subscription, User user, Payment payment, string invoiceNumber)
    {
        var invoicesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "invoices");
        Directory.CreateDirectory(invoicesFolder);

        var fileName = $"{invoiceNumber}.pdf";
        var fullPath = Path.Combine(invoicesFolder, fileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(32);
                page.Header().Text($"AfneyGym Makbuz - {invoiceNumber}").SemiBold().FontSize(18);
                page.Content().Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Text($"Uye: {user.FirstName} {user.LastName}");
                    col.Item().Text($"E-posta: {user.Email}");
                    col.Item().Text($"Paket: {subscription.PlanName}");
                    col.Item().Text($"Donem: {subscription.StartDate:dd.MM.yyyy} - {subscription.EndDate:dd.MM.yyyy}");
                    col.Item().Text($"Tutar: {payment.Amount:N2} {payment.Currency}");
                    col.Item().Text($"Odeme Yontemi: {payment.Provider}");
                    col.Item().Text($"Tarih: {payment.PaidAt:dd.MM.yyyy HH:mm}");
                });
                page.Footer().AlignCenter().Text("AfneyGym - Dijital Makbuz");
            });
        }).GeneratePdf(fullPath);

        return $"/invoices/{fileName}";
    }

    // Auto-renew mantigi SubscriptionRenewalService icine alindi.
    #endregion
}
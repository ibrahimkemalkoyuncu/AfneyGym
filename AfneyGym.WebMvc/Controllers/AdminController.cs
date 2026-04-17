using AfneyGym.Domain.Interfaces;
using AfneyGym.Domain.Entities;
using AfneyGym.Common.DTOs;
using AfneyGym.Common.Enums; // Mühendislik Düzeltmesi: Merkezi Enum Referansı
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfneyGym.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfneyGym.WebMvc.Controllers;

[Authorize(Roles = "Admin,Owner")]
public class AdminController : Controller
{
    // FIELD MANAGEMENT PROTOKOLÜ: Tekil ve Readonly Alan Tanımları
    private readonly IDashboardService _dashboardService;
    private readonly ITrainerService _trainerService;
    private readonly ILessonService _lessonService;
    private readonly IMemberLifecycleService _memberLifecycleService;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(
        IDashboardService dashboardService,
        ITrainerService trainerService,
        ILessonService lessonService,
        IMemberLifecycleService memberLifecycleService,
        AppDbContext context,
        IWebHostEnvironment webHostEnvironment)
    {
        _dashboardService = dashboardService;
        _trainerService = trainerService;
        _lessonService = lessonService;
        _memberLifecycleService = memberLifecycleService;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    #region Dashboard
    public async Task<IActionResult> Dashboard()
    {
        // DashboardSummaryDto (Hafif DTO) üzerinden projeksiyon yapılmış veriler çekilir
        var stats = await _dashboardService.GetSummaryStatsAsync();
        return View(stats);
    }
    #endregion

    #region Analytics
    public async Task<IActionResult> Analytics()
    {
        var analytics = await _dashboardService.GetAnalyticsAsync();
        return View(analytics);
    }
    #endregion

    #region Üye Yönetimi (Members & Edit)
    public async Task<IActionResult> Members()
    {
        // Sadece silinmemiş kullanıcıları listele (Soft-Delete Check)
        var members = await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        return View(members);
    }

    [HttpGet]
    public async Task<IActionResult> MemberDetail(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Gym)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user == null) return NotFound();

        var stage = await _memberLifecycleService.GetCurrentStageAsync(id);
        var monthlyCheckIn = await _memberLifecycleService.GetMonthlyCheckInCountAsync(id);
        var checkIns = await _memberLifecycleService.GetRecentCheckInsAsync(id);
        var metrics = await _memberLifecycleService.GetBodyMetricsAsync(id, 12);
        var latestMetric = await _memberLifecycleService.GetLatestBodyMetricSummaryAsync(id);
        var goals = await _memberLifecycleService.GetActiveGoalsAsync(id);

        ViewBag.Member = user;
        ViewBag.Stage = stage.ToString();
        ViewBag.MonthlyCheckIn = monthlyCheckIn;
        ViewBag.CheckIns = checkIns;
        ViewBag.Metrics = metrics;
        ViewBag.LatestMetric = latestMetric;
        ViewBag.Goals = goals;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInMember(Guid id)
    {
        var success = await _memberLifecycleService.CheckInAsync(id);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Uyeye check-in kaydi eklendi."
            : "Check-in kaydi olusturulamadi. Uyenin acik bir seansi olabilir.";

        return RedirectToAction(nameof(MemberDetail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBodyMetric(Guid id, BodyMetricCreateDto dto)
    {
        if (dto.Weight <= 0)
        {
            TempData["ErrorMessage"] = "Kilo degeri 0'dan buyuk olmali.";
            return RedirectToAction(nameof(MemberDetail), new { id });
        }

        await _memberLifecycleService.AddBodyMetricAsync(id, dto);
        TempData["SuccessMessage"] = "Vucut olcumu kaydedildi.";
        return RedirectToAction(nameof(MemberDetail), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> AtRiskMembers()
    {
        var ids = await _memberLifecycleService.GetAtRiskMembersAsync();
        var members = await _context.Users
            .Where(u => ids.Contains(u.Id) && !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ToListAsync();

        return View(members);
    }

    [HttpGet]
    public async Task<IActionResult> EditMember(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var pendingSubscriptions = await _context.Subscriptions
            .Where(s => s.UserId == id && s.Status == SubscriptionStatus.Pending && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var latestSubscription = await _context.Subscriptions
            .Where(s => s.UserId == id && !s.IsDeleted)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        ViewBag.PendingSubscriptions = pendingSubscriptions;
        ViewBag.LatestSubscription = latestSubscription;

        // PROJECTION: Entity -> DTO (Mühendislik Protokolü: Veri Sızıntısını Önler)
        var updateDto = new UserUpdateDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role, // Common.Enums.UserRole ile tam uyumlu
            GymId = user.GymId
        };

        await PrepareGymsDropdown();
        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMember(UserUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            await PrepareGymsDropdown();
            return View(updateDto);
        }

        var user = await _context.Users.FindAsync(updateDto.Id);
        if (user == null) return NotFound();

        // Business Logic: E-posta Tekilliği Kontrolü
        if (user.Email != updateDto.Email)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == updateDto.Email && u.Id != updateDto.Id);
            if (exists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten başka bir kullanıcı tarafından kullanılıyor.");
                await PrepareGymsDropdown();
                return View(updateDto);
            }
        }

        // MAPPING: DTO -> Entity (Tipler %100 Senkronize)
        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.Email = updateDto.Email;
        user.Role = updateDto.Role;
        user.GymId = updateDto.GymId;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Üye bilgileri başarıyla güncellendi.";
        return RedirectToAction(nameof(Members));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsDeleted = true; // Mühendislik Standardı: Soft Delete
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Üye başarıyla silindi.";
        return RedirectToAction(nameof(Members));
    }
    #endregion

    #region Eğitmen Yönetimi (Trainers)
    public async Task<IActionResult> Trainers()
    {
        var trainers = await _trainerService.GetAllAsync();
        return View(trainers);
    }

    [HttpGet]
    public IActionResult CreateTrainer() => View();

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> CreateTrainer(Trainer trainer, IFormFile? imageFile)
    //{
    //    if (ModelState.IsValid)
    //    {

    //        // Email Mükerrerlik Kontrolü

    #region Ders Yönetimi - Owner Panel
    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("/owner/classes")]
    public async Task<IActionResult> ManageClasses()
    {
        var lessons = await _lessonService.GetAllWithTrainersAsync();
        return View(lessons);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("/owner/classes/create")]
    public async Task<IActionResult> OwnerCreateLesson()
    {
        await PrepareLessonDropdowns();
        return View("CreateLesson");
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("/owner/classes/{id}/attendees")]
    public async Task<IActionResult> ClassAttendees(Guid id)
    {
        var lesson = await _lessonService.GetByIdWithAttendeesAsync(id);
        if (lesson == null) return NotFound();
        return View(lesson);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("/owner/classes/{id}/attendance")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAttendance(Guid id, Guid attendeeId, bool attended)
    {
        var success = await _lessonService.MarkAttendanceAsync(attendeeId, attended);
        if (success)
            TempData["SuccessMessage"] = "Yoklama başarıyla kaydedildi.";
        else
            TempData["ErrorMessage"] = "Yoklama kaydedilemedi.";

        return RedirectToAction(nameof(ClassAttendees), new { id });
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpGet("/owner/classes/{id}/edit")]
    public async Task<IActionResult> EditLesson(Guid id)
    {
        var lesson = await _lessonService.GetByIdAsync(id);
        if (lesson == null || lesson.IsDeleted) return NotFound();

        await PrepareLessonDropdowns();
        return View(lesson);
    }

    [Authorize(Roles = "Admin,Owner")]
    [HttpPost("/owner/classes/{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLesson(Guid id, Lesson lesson)
    {
        if (id != lesson.Id) return BadRequest();

        if (lesson.StartTime >= lesson.EndTime)
            ModelState.AddModelError("EndTime", "Bitiş saati başlangıç saatinden sonra olmalıdır.");

        if (!ModelState.IsValid)
        {
            await PrepareLessonDropdowns();
            return View(lesson);
        }

        var existingLesson = await _lessonService.GetByIdAsync(id);
        if (existingLesson == null || existingLesson.IsDeleted) return NotFound();

        existingLesson.Name = lesson.Name;
        existingLesson.Description = lesson.Description;
        existingLesson.StartTime = lesson.StartTime;
        existingLesson.EndTime = lesson.EndTime;
        existingLesson.Capacity = lesson.Capacity;
        existingLesson.TrainerId = lesson.TrainerId;
        existingLesson.GymId = lesson.GymId;
        existingLesson.UpdatedAt = DateTime.UtcNow;

        var updated = await _lessonService.UpdateAsync(existingLesson);
        if (!updated)
            TempData["ErrorMessage"] = "Ders güncellenemedi. Eğitmen saat çakışması veya veri hatası olabilir.";
        else
            TempData["SuccessMessage"] = "Ders başarıyla güncellendi.";

        return RedirectToAction(nameof(ManageClasses));
    }
    #endregion
    //        var exists = await _context.Trainers.AnyAsync(t => t.Email == trainer.Email && !t.IsDeleted);
    //        if (exists)
    //        {
    //            ModelState.AddModelError("Email", "Bu e-posta adresiyle kayıtlı bir eğitmen zaten var.");
    //            return View(trainer);
    //        }

    //        if (imageFile != null && imageFile.Length > 0)
    //        {
    //            var extension = Path.GetExtension(imageFile.FileName).ToLower();
    //            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
    //            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "trainers");

    //            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

    //            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
    //            using (var fileStream = new FileStream(filePath, FileMode.Create))
    //            {
    //                await imageFile.CopyToAsync(fileStream);
    //            }
    //            trainer.ImageUrl = $"/uploads/trainers/{uniqueFileName}";
    //        }
    //        else
    //        {
    //            trainer.ImageUrl = "/img/default-trainer.png";
    //        }

    //        await _trainerService.CreateAsync(trainer);
    //        return RedirectToAction(nameof(Trainers));
    //    }
    //    return View(trainer);
    //}


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrainer(Trainer trainer, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            // Email Mükerrerlik Kontrolü
            var exists = await _context.Trainers.AnyAsync(t => t.Email == trainer.Email && !t.IsDeleted);
            if (exists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresiyle kayıtlı bir eğitmen zaten var.");
                return View(trainer);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/trainers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await imageFile.CopyToAsync(fileStream); }
                trainer.ImageUrl = "/uploads/trainers/" + uniqueFileName;
            }
            else
            {
                trainer.ImageUrl = "/img/default-trainer.png";
            }

            await _trainerService.CreateAsync(trainer);
            return RedirectToAction(nameof(Trainers));
        }
        return View(trainer);
    }


    // --- YENİ: EĞİTMEN DÜZENLEME (GET) ---
    [HttpGet]
    public async Task<IActionResult> EditTrainer(Guid id)
    {
        var trainer = await _trainerService.GetByIdAsync(id);
        if (trainer == null) return NotFound();
        return View(trainer);
    }

    // --- YENİ: EĞİTMEN DÜZENLEME (POST) ---
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> EditTrainer(Trainer trainer, IFormFile? imageFile)
    //{
    //    if (ModelState.IsValid)
    //    {
    //        var existingTrainer = await _trainerService.GetByIdAsync(trainer.Id);
    //        if (existingTrainer == null) return NotFound();

    //        if (imageFile != null && imageFile.Length > 0)
    //        {
    //            // Eski resmi sil (Default değilse)
    //            if (!string.IsNullOrEmpty(existingTrainer.ImageUrl) && !existingTrainer.ImageUrl.Contains("default-trainer.png"))
    //            {
    //                var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingTrainer.ImageUrl.TrimStart('/'));
    //                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
    //            }

    //            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
    //            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/trainers", uniqueFileName);
    //            using (var fileStream = new FileStream(filePath, FileMode.Create)) { await imageFile.CopyToAsync(fileStream); }
    //            existingTrainer.ImageUrl = "/uploads/trainers/" + uniqueFileName;
    //        }

    //        existingTrainer.FullName = trainer.FullName;
    //        existingTrainer.Specialty = trainer.Specialty;
    //        existingTrainer.Bio = trainer.Bio;
    //        existingTrainer.UpdatedAt = DateTime.Now;

    //        await _trainerService.UpdateAsync(existingTrainer);
    //        return RedirectToAction(nameof(Trainers));
    //    }
    //    return View(trainer);
    //}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrainer(Trainer trainer, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            var existingTrainer = await _trainerService.GetByIdAsync(trainer.Id);
            if (existingTrainer == null) return NotFound();

            // Email Mükerrerlik Kontrolü (Kendisi hariç)
            var exists = await _context.Trainers.AnyAsync(t => t.Email == trainer.Email && t.Id != trainer.Id && !t.IsDeleted);
            if (exists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi başka bir eğitmen tarafından kullanılıyor.");
                return View(trainer);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                // Eski resmi sil
                if (!string.IsNullOrEmpty(existingTrainer.ImageUrl) && !existingTrainer.ImageUrl.Contains("default-trainer.png"))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingTrainer.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/trainers", uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await imageFile.CopyToAsync(fileStream); }
                existingTrainer.ImageUrl = "/uploads/trainers/" + uniqueFileName;
            }

            // Veri Güncelleme
            existingTrainer.FullName = trainer.FullName;
            existingTrainer.Email = trainer.Email; // GÜNCELLENDİ
            existingTrainer.Specialty = trainer.Specialty;
            existingTrainer.Bio = trainer.Bio;
            existingTrainer.UpdatedAt = DateTime.Now;

            await _trainerService.UpdateAsync(existingTrainer);
            return RedirectToAction(nameof(Trainers));
        }
        return View(trainer);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrainer(Guid id)
    {
        var trainer = await _trainerService.GetByIdAsync(id);
        if (trainer == null) return NotFound();

        // File System Cleanup: Fiziksel Resim Dosyasını Temizle
        if (!string.IsNullOrEmpty(trainer.ImageUrl) && !trainer.ImageUrl.Contains("default-trainer.png"))
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, trainer.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
        }

        await _trainerService.DeleteAsync(id);
        return RedirectToAction(nameof(Trainers));
    }
    #endregion

    #region Ders Yönetimi (Lessons)
    public async Task<IActionResult> Lessons()
    {
        var lessons = await _lessonService.GetAllWithTrainersAsync();
        return View(lessons);
    }

    [HttpGet]
    public async Task<IActionResult> CreateLesson()
    {
        await PrepareLessonDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(Lesson lesson)
    {
        // Tarih Mantığı Doğrulaması
        if (lesson.StartTime >= lesson.EndTime)
            ModelState.AddModelError("EndTime", "Bitiş saati başlangıç saatinden sonra olmalıdır.");

        if (ModelState.IsValid)
        {
            var created = await _lessonService.CreateAsync(lesson);
            if (!created)
            {
                ModelState.AddModelError(string.Empty, "Ders oluşturulamadı. Eğitmen seçilen saat aralığında başka bir derste olabilir.");
                await PrepareLessonDropdowns();
                return View(lesson);
            }

            return RedirectToAction(nameof(ManageClasses));
        }

        await PrepareLessonDropdowns();
        return View(lesson);
    }
    #endregion

    #region DRY HELPERS (Yardımcı Metotlar)
    private async Task PrepareLessonDropdowns()
    {
        var trainers = await _trainerService.GetAllAsync();
        var gyms = await _context.Gyms.ToListAsync();
        ViewBag.Trainers = new SelectList(trainers, "Id", "FullName");
        ViewBag.Gyms = new SelectList(gyms, "Id", "Name");
    }

    private async Task PrepareGymsDropdown()
    {
        var gyms = await _context.Gyms.ToListAsync();
        ViewBag.Gyms = new SelectList(gyms, "Id", "Name");
    }
    #endregion
}
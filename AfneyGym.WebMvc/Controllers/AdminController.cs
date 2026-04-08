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

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // FIELD MANAGEMENT PROTOKOLÜ: Tekil ve Readonly Alan Tanımları
    private readonly IDashboardService _dashboardService;
    private readonly ITrainerService _trainerService;
    private readonly ILessonService _lessonService;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(
        IDashboardService dashboardService,
        ITrainerService trainerService,
        ILessonService lessonService,
        AppDbContext context,
        IWebHostEnvironment webHostEnvironment)
    {
        _dashboardService = dashboardService;
        _trainerService = trainerService;
        _lessonService = lessonService;
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
    public async Task<IActionResult> EditMember(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrainer(Trainer trainer, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "trainers");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                trainer.ImageUrl = $"/uploads/trainers/{uniqueFileName}";
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
            await _lessonService.CreateAsync(lesson);
            return RedirectToAction(nameof(Lessons));
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
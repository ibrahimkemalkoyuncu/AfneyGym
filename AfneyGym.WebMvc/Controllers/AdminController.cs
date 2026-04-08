using AfneyGym.Domain.Interfaces;
using AfneyGym.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfneyGym.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfneyGym.WebMvc.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
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

    // --- REGRESSION CHECK: Tüm mevcut metodlar (Dashboard, Members, Trainers) korunmuştur ---
    public async Task<IActionResult> Dashboard()
    {
        var stats = await _dashboardService.GetSummaryStatsAsync();
        return View(stats);
    }

    public async Task<IActionResult> Members()
    {
        var members = await _context.Users.Where(u => !u.IsDeleted).OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View(members);
    }

    public async Task<IActionResult> Trainers()
    {
        var trainers = await _trainerService.GetAllAsync();
        return View(trainers);
    }

    [HttpGet] public IActionResult CreateTrainer() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrainer(Trainer trainer, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension)) { ModelState.AddModelError("imageFile", "Format hatası."); return View(trainer); }

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "trainers");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + extension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await imageFile.CopyToAsync(fileStream); }
                trainer.ImageUrl = "/uploads/trainers/" + uniqueFileName;
            }
            else { trainer.ImageUrl = "/img/default-trainer.png"; }
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTrainer(Trainer trainer, IFormFile? imageFile)
    {
        if (ModelState.IsValid)
        {
            var existingTrainer = await _trainerService.GetByIdAsync(trainer.Id);
            if (existingTrainer == null) return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                // Eski resmi sil (Default değilse)
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

            existingTrainer.FullName = trainer.FullName;
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
        if (!string.IsNullOrEmpty(trainer.ImageUrl) && !trainer.ImageUrl.Contains("default-trainer.png"))
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, trainer.ImageUrl.TrimStart('/'));
            try { if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath); } catch { }
        }
        await _trainerService.DeleteAsync(id);
        return RedirectToAction(nameof(Trainers));
    }

    // --- DERS YÖNETİMİ ---
    public async Task<IActionResult> Lessons()
    {
        var lessons = await _lessonService.GetAllWithTrainersAsync();
        return View(lessons);
    }

    [HttpGet]
    public async Task<IActionResult> CreateLesson()
    {
        var trainers = await _trainerService.GetAllAsync();
        ViewBag.Trainers = new SelectList(trainers, "Id", "FullName");
        var gyms = await _context.Gyms.ToListAsync();
        ViewBag.Gyms = new SelectList(gyms, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLesson(Lesson lesson)
    {
        if (lesson.StartTime >= lesson.EndTime)
            ModelState.AddModelError("EndTime", "Bitiş saati başlangıçtan büyük olmalıdır.");

        if (ModelState.IsValid)
        {
            var result = await _lessonService.CreateAsync(lesson);
            if (result) return RedirectToAction(nameof(Lessons));

            // ÇAKIŞMA DURUMUNDA HATA MESAJI (YENİ)
            ModelState.AddModelError(string.Empty, "Seçilen eğitmen bu saat aralığında başka bir derstedir.");
        }

        var trainers = await _trainerService.GetAllAsync();
        ViewBag.Trainers = new SelectList(trainers, "Id", "FullName");
        var gyms = await _context.Gyms.ToListAsync();
        ViewBag.Gyms = new SelectList(gyms, "Id", "Name");
        return View(lesson);
    }
}
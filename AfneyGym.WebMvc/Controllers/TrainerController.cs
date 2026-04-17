using AfneyGym.Common.Enums;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AfneyGym.WebMvc.Controllers;

[Authorize(Roles = "Trainer")]
public class TrainerController : Controller
{
    private readonly AppDbContext _context;

    public TrainerController(AppDbContext context)
    {
        _context = context;
    }

    #region Eğitmen Ders Programı
    [HttpGet]
    public async Task<IActionResult> MyLessons()
    {
        var trainerEmail = User.FindFirstValue(ClaimTypes.Email);

        // Mühendislik Kontrolü: Giriş yapan kullanıcının eğitmen profiliyle eşleşmesi
        var trainer = await _context.Trainers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Email == trainerEmail && !t.IsDeleted);

        if (trainer == null) return NotFound("Eğitmen profiliniz sistemde aktif görünmüyor.");

        var lessons = await _context.Lessons
            .AsNoTracking()
            .Include(l => l.Attendees)
            .Where(l => l.TrainerId == trainer.Id && !l.IsDeleted)
            .OrderBy(l => l.StartTime)
            .ToListAsync();

        return View(lessons);
    }
    #endregion

    #region Yoklama Yönetimi
    [HttpGet]
    public async Task<IActionResult> Attendance(Guid lessonId)
    {
        var trainerEmail = User.FindFirstValue(ClaimTypes.Email);

        // Security Check: Eğitmen sadece kendi dersinin yoklamasını görebilir.
        var lesson = await _context.Lessons
            .Include(l => l.Attendees!).ThenInclude(a => a.User)
            .Include(l => l.Trainer)
            .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

        if (lesson == null) return NotFound();
        if (lesson.Trainer?.Email != trainerEmail) return Forbid(); // Yetkisiz erişim engellendi

        return View(lesson);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitAttendance(Guid lessonId, List<Guid> attendedUserIds)
    {
        var trainerEmail = User.FindFirstValue(ClaimTypes.Email);

        // Security Check: POST isteği yetkili eğitmen tarafından mı yapıldı?
        var lesson = await _context.Lessons
            .Include(l => l.Trainer)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null || lesson.Trainer?.Email != trainerEmail) return Forbid();

        var attendees = await _context.LessonAttendees
            .Where(a => a.LessonId == lessonId)
            .ToListAsync();

        // DRY HELPERS: Yoklama durumlarını güncelle
        foreach (var attendee in attendees)
        {
            // Liste null gelirse (hiç kimse seçilmezse) hata almamak için kontrol
            attendee.IsAttended = attendedUserIds != null && attendedUserIds.Contains(attendee.UserId);
            attendee.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Yoklama başarıyla sisteme işlendi.";

        return RedirectToAction(nameof(MyLessons));
    }
    #endregion


    //#region Ders Programı
    //// Eğitmenin haftalık/günlük ders programı
    //public async Task<IActionResult> MyLessons()
    //{
    //    var trainerEmail = User.FindFirstValue(ClaimTypes.Email);

    //    // Eğitmenin kendi kaydını bul
    //    var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Email == trainerEmail && !t.IsDeleted);
    //    if (trainer == null) return NotFound("Eğitmen kaydı bulunamadı.");

    //    var lessons = await _context.Lessons
    //        .Include(l => l.Attendees)
    //        .Where(l => l.TrainerId == trainer.Id && !l.IsDeleted)
    //        .OrderBy(l => l.StartTime)
    //        .ToListAsync();

    //    return View(lessons);
    //}
    //#endregion

    //#region Yoklama İşlemleri
    //// Yoklama sayfası (Belirli bir ders için)
    //public async Task<IActionResult> Attendance(Guid lessonId)
    //{
    //    var lesson = await _context.Lessons
    //        .Include(l => l.Attendees!)
    //            .ThenInclude(a => a.User)
    //        .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

    //    if (lesson == null) return NotFound();

    //    return View(lesson);
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> SubmitAttendance(Guid lessonId, List<Guid> attendedUserIds)
    //{
    //    var attendees = await _context.LessonAttendees
    //        .Where(a => a.LessonId == lessonId)
    //        .ToListAsync();

    //    foreach (var attendee in attendees)
    //    {
    //        // MÜHENDİSLİK MANTIĞI: Checkbox'tan gelen ID listesinde varsa true, yoksa false yap
    //        attendee.IsAttended = attendedUserIds != null && attendedUserIds.Contains(attendee.UserId);
    //        attendee.UpdatedAt = DateTime.Now;
    //    }

    //    await _context.SaveChangesAsync();
    //    TempData["SuccessMessage"] = "Yoklama kaydı güncellendi.";
    //    return RedirectToAction(nameof(MyLessons));
    //}

    //#endregion
}
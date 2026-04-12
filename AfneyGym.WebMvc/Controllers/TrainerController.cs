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

    #region Ders Programı
    // Eğitmenin haftalık/günlük ders programı
    public async Task<IActionResult> MyLessons()
    {
        var trainerEmail = User.FindFirstValue(ClaimTypes.Email);

        // Eğitmenin kendi kaydını bul
        var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Email == trainerEmail && !t.IsDeleted);
        if (trainer == null) return NotFound("Eğitmen kaydı bulunamadı.");

        var lessons = await _context.Lessons
            .Include(l => l.Attendees)
            .Where(l => l.TrainerId == trainer.Id && !l.IsDeleted)
            .OrderBy(l => l.StartTime)
            .ToListAsync();

        return View(lessons);
    }
    #endregion

    #region Yoklama İşlemleri
    // Yoklama sayfası (Belirli bir ders için)
    public async Task<IActionResult> Attendance(Guid lessonId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Attendees!)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

        if (lesson == null) return NotFound();

        return View(lesson);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitAttendance(Guid lessonId, List<Guid> attendedUserIds)
    {
        var attendees = await _context.LessonAttendees
            .Where(a => a.LessonId == lessonId)
            .ToListAsync();

        foreach (var attendee in attendees)
        {
            // MÜHENDİSLİK MANTIĞI: Checkbox'tan gelen ID listesinde varsa true, yoksa false yap
            attendee.IsAttended = attendedUserIds != null && attendedUserIds.Contains(attendee.UserId);
            attendee.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Yoklama kaydı güncellendi.";
        return RedirectToAction(nameof(MyLessons));
    }

    #endregion
}
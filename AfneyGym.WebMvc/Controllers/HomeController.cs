using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AfneyGym.WebMvc.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var lessons = await _context.Lessons
            .Include(l => l.Trainer)
            .Include(l => l.Attendees)
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.StartTime)
            .ToListAsync();

        return View(lessons);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinLesson(Guid lessonId)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (userIdClaim == null) return RedirectToAction("Login", "Account");
        var userId = Guid.Parse(userIdClaim);

        var lesson = await _context.Lessons.Include(l => l.Attendees).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return NotFound();

        if (lesson.Attendees != null && lesson.Attendees.Count >= lesson.Capacity)
        {
            TempData["ErrorMessage"] = "Üzgünüz, kontenjan doldu.";
            return RedirectToAction("Index");
        }

        if (await _context.LessonAttendees.AnyAsync(la => la.LessonId == lessonId && la.UserId == userId))
        {
            TempData["ErrorMessage"] = "Bu derse zaten katıldınız.";
            return RedirectToAction("Index");
        }

        var attendee = new LessonAttendee { LessonId = lessonId, UserId = userId, CreatedAt = DateTime.Now };
        _context.LessonAttendees.Add(attendee);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Derse başarıyla kayıt oldunuz!";
        return RedirectToAction("Index");
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

        // Kullanıcının bu derse ait kaydını bul
        var attendance = await _context.LessonAttendees
            .FirstOrDefaultAsync(la => la.LessonId == lessonId && la.UserId == userId);

        if (attendance != null)
        {
            _context.LessonAttendees.Remove(attendance);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ders katılımınız iptal edildi.";
        }
        else
        {
            TempData["ErrorMessage"] = "İptal edilecek bir kayıt bulunamadı.";
        }

        return RedirectToAction("Index");
    }
}
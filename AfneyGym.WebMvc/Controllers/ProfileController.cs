using AfneyGym.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AfneyGym.WebMvc.Controllers;

[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
public class ProfileController : Controller
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

        var userId = Guid.Parse(userIdClaim);

        // Kullanıcı verilerini ve katıldığı dersleri (Eğitmenleri ile birlikte) çekiyoruz
        var userProfile = await _context.Users
            .Include(u => u.Gym)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var myLessons = await _context.LessonAttendees
            .Include(la => la.Lesson)
                .ThenInclude(l => l.Trainer)
            .Where(la => la.UserId == userId)
            .OrderByDescending(la => la.Lesson.StartTime)
            .ToListAsync();

        ViewBag.UserProfile = userProfile;
        return View(myLessons);
    }
}
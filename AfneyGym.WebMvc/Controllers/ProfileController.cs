//using AfneyGym.Data.Context;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace AfneyGym.WebMvc.Controllers;

//[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
//public class ProfileController : Controller
//{
//    private readonly AppDbContext _context;

//    public ProfileController(AppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IActionResult> Index()
//    {
//        var userIdClaim = User.FindFirst("UserId")?.Value;
//        if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

//        var userId = Guid.Parse(userIdClaim);

//        // Kullanıcı verilerini ve katıldığı dersleri (Eğitmenleri ile birlikte) çekiyoruz
//        var userProfile = await _context.Users
//            .Include(u => u.Gym)
//            .FirstOrDefaultAsync(u => u.Id == userId);

//        var myLessons = await _context.LessonAttendees
//            .Include(la => la.Lesson)
//                .ThenInclude(l => l.Trainer)
//            .Where(la => la.UserId == userId)
//            .OrderByDescending(la => la.Lesson.StartTime)
//            .ToListAsync();

//        ViewBag.UserProfile = userProfile;
//        return View(myLessons);
//    }
//}

using AfneyGym.Common.DTOs;
using AfneyGym.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AfneyGym.WebMvc.Controllers;

[Authorize(Roles = "Member")]
public class ProfileController : Controller
{
    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        // Üyenin katılım verilerini ve ders detaylarını çek
        var attendanceData = await _context.LessonAttendees
            .AsNoTracking()
            .Include(a => a.Lesson).ThenInclude(l => l!.Trainer)
            .Where(a => a.UserId == userId && !a.IsDeleted && a.Lesson != null)
            .OrderByDescending(a => a.Lesson!.StartTime)
            .ToListAsync();

        var model = new MemberProfileDto
        {
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            JoinDate = user.CreatedAt,
            TotalRegisteredLessons = attendanceData.Count,
            AttendedLessonsCount = attendanceData.Count(a => a.IsAttended),
            AttendanceHistory = attendanceData.Select(a => new MemberAttendanceHistoryDto
            {
                LessonName = a.Lesson?.Name ?? "Belirtilmedi",
                LessonDate = a.Lesson?.StartTime ?? DateTime.MinValue,
                TrainerName = a.Lesson?.Trainer?.FullName ?? "Belirtilmedi",
                IsAttended = a.IsAttended
            }).ToList()
        };

        return View(model);
    }
}
using AfneyGym.Domain.Interfaces;
using AfneyGym.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AfneyGym.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.WebMvc.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // CS0229 ÇÖZÜMÜ: Sadece bir tane private readonly alan olmalı.
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

    // REGRESSION CHECK: Tüm Admin aksiyonları tek bir servis örneği kullanır.
    public async Task<IActionResult> Dashboard()
    {
        var summary = await _dashboardService.GetSummaryStatsAsync();
        return View(summary);
    }

    public async Task<IActionResult> Members() => View(await _context.Users.Where(u => !u.IsDeleted).OrderByDescending(u => u.CreatedAt).ToListAsync());
    public async Task<IActionResult> Trainers() => View(await _trainerService.GetAllAsync());
    public async Task<IActionResult> Lessons() => View(await _lessonService.GetAllWithTrainersAsync());

    // ... Diğer metodlar (Create/Edit/Delete) korunuyor ...
}
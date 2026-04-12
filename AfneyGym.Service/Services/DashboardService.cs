using AfneyGym.Common.DTOs;
using AfneyGym.Common.Enums;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.Service.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    //public async Task<DashboardSummaryDto> GetSummaryStatsAsync()
    //{
    //    var totalMembers = await _context.Users
    //        .CountAsync(u => !u.IsDeleted && u.Role == UserRole.Member);

    //    var activeSubs = await _context.Subscriptions
    //        .CountAsync(s => s.EndDate > DateTime.Now && !s.IsDeleted);

    //    var trainers = await _context.Trainers.CountAsync(t => !t.IsDeleted);

    //    // MÜHENDİSLİK DÜZELTMESİ: Entity -> DTO Projeksiyonu
    //    var recentMembers = await _context.Users
    //        .Where(u => !u.IsDeleted && u.Role == UserRole.Member)
    //        .OrderByDescending(u => u.CreatedAt)
    //        .Take(5)
    //        .Select(u => new RecentMemberDto
    //        {
    //            FullName = u.FirstName + " " + u.LastName,
    //            Email = u.Email,
    //            CreatedAt = u.CreatedAt
    //        }).ToListAsync();

    //    var todaysLessons = await _context.Lessons
    //        .Include(l => l.Trainer)
    //        .Include(l => l.Attendees)
    //        .Where(l => l.StartTime.Date == DateTime.Today && !l.IsDeleted)
    //        .OrderBy(l => l.StartTime)
    //        .Select(l => new TodaysLessonDto
    //        {
    //            Name = l.Name,
    //            TrainerName = l.Trainer != null ? l.Trainer.FullName : "Belirtilmemiş",
    //            StartTime = l.StartTime,
    //            AttendeeCount = l.Attendees.Count,
    //            Capacity = l.Capacity
    //        }).ToListAsync();

    //    return new DashboardSummaryDto
    //    {
    //        TotalMembers = totalMembers,
    //        ActiveSubscriptions = activeSubs,
    //        TotalTrainers = trainers,
    //        TodayLessonsCount = todaysLessons.Count,
    //        RecentMembers = recentMembers,
    //        TodaysLessons = todaysLessons
    //    };
    //}


    public async Task<DashboardSummaryDto> GetSummaryStatsAsync()
    {
        var today = DateTime.Today;

        var dto = new DashboardSummaryDto
        {
            // Sayaçlar (Real-time Counts)
            TotalMembers = await _context.Users.CountAsync(u => !u.IsDeleted),

            ActiveSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.Now),

            TotalTrainers = await _context.Trainers.CountAsync(t => !t.IsDeleted),

            PendingSubscriptionCount = await _context.Subscriptions
                .CountAsync(s => s.Status == SubscriptionStatus.Pending && !s.IsDeleted),

            TodayLessonsCount = await _context.Lessons
                .CountAsync(l => l.StartTime.Date == today && !l.IsDeleted),

            // Son Kayıt Olan 5 Üye
            RecentMembers = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentMemberDto
                {
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                }).ToListAsync(),

            // Bugünün Ders Programı ve Doluluk Oranları
            TodaysLessons = await _context.Lessons
                .Include(l => l.Trainer)
                .Include(l => l.Attendees)
                .Where(l => l.StartTime.Date == today && !l.IsDeleted)
                .OrderBy(l => l.StartTime)
                .Select(l => new TodaysLessonDto
                {
                    Name = l.Name,
                    TrainerName = l.Trainer != null ? l.Trainer.FullName : "Atanmadı",
                    StartTime = l.StartTime,
                    Capacity = l.Capacity,
                    AttendeeCount = l.Attendees.Count
                }).ToListAsync()
        };

        return dto;
    }

}
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


    //public async Task<DashboardSummaryDto> GetSummaryStatsAsync()
    //{
    //    var today = DateTime.Today;

    //    var dto = new DashboardSummaryDto
    //    {
    //        // Sayaçlar (Real-time Counts)
    //        TotalMembers = await _context.Users.CountAsync(u => !u.IsDeleted),

    //        ActiveSubscriptions = await _context.Subscriptions
    //            .CountAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate > DateTime.Now),

    //        TotalTrainers = await _context.Trainers.CountAsync(t => !t.IsDeleted),

    //        PendingSubscriptionCount = await _context.Subscriptions
    //            .CountAsync(s => s.Status == SubscriptionStatus.Pending && !s.IsDeleted),

    //        TodayLessonsCount = await _context.Lessons
    //            .CountAsync(l => l.StartTime.Date == today && !l.IsDeleted),

    //        // Son Kayıt Olan 5 Üye
    //        RecentMembers = await _context.Users
    //            .Where(u => !u.IsDeleted)
    //            .OrderByDescending(u => u.CreatedAt)
    //            .Take(5)
    //            .Select(u => new RecentMemberDto
    //            {
    //                FullName = $"{u.FirstName} {u.LastName}",
    //                Email = u.Email,
    //                CreatedAt = u.CreatedAt
    //            }).ToListAsync(),

    //        // Bugünün Ders Programı ve Doluluk Oranları
    //        TodaysLessons = await _context.Lessons
    //            .Include(l => l.Trainer)
    //            .Include(l => l.Attendees)
    //            .Where(l => l.StartTime.Date == today && !l.IsDeleted)
    //            .OrderBy(l => l.StartTime)
    //            .Select(l => new TodaysLessonDto
    //            {
    //                Name = l.Name,
    //                TrainerName = l.Trainer != null ? l.Trainer.FullName : "Atanmadı",
    //                StartTime = l.StartTime,
    //                Capacity = l.Capacity,
    //                AttendeeCount = l.Attendees.Count
    //            }).ToListAsync()
    //    };

    //    return dto;
    //}


    //public async Task<DashboardSummaryDto> GetSummaryStatsAsync()
    //{
    //    var today = DateTime.Today;
    //    var now = DateTime.Now;

    //    // MÜHENDİSLİK NOTU: Veritabanı yükünü azaltmak için AsNoTracking kullanıldı.
    //    var query = _context.Subscriptions.AsNoTracking();

    //    var dto = new DashboardSummaryDto
    //    {
    //        // --- CANLI SAYAÇLAR ---
    //        TotalMembers = await _context.Users.CountAsync(u => !u.IsDeleted),

    //        ActiveSubscriptions = await query
    //            .CountAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate > now),

    //        TotalTrainers = await _context.Trainers.CountAsync(t => !t.IsDeleted),

    //        PendingSubscriptionCount = await query
    //            .CountAsync(s => s.Status == SubscriptionStatus.Pending && !s.IsDeleted),

    //        TodayLessonsCount = await _context.Lessons
    //            .CountAsync(l => l.StartTime.Date == today && !l.IsDeleted),

    //        // --- PROJEKSİYON: SON KAYITLAR ---
    //        RecentMembers = await _context.Users
    //            .AsNoTracking()
    //            .Where(u => !u.IsDeleted)
    //            .OrderByDescending(u => u.CreatedAt)
    //            .Take(5)
    //            .Select(u => new RecentMemberDto
    //            {
    //                FullName = $"{u.FirstName} {u.LastName}",
    //                Email = u.Email,
    //                CreatedAt = u.CreatedAt
    //            }).ToListAsync(),

    //        // --- PROJEKSİYON: BUGÜNÜN AKIŞI ---
    //        TodaysLessons = await _context.Lessons
    //            .AsNoTracking()
    //            .Include(l => l.Trainer)
    //            .Include(l => l.Attendees)
    //            .Where(l => l.StartTime.Date == today && !l.IsDeleted)
    //            .OrderBy(l => l.StartTime)
    //            .Select(l => new TodaysLessonDto
    //            {
    //                Name = l.Name,
    //                TrainerName = l.Trainer != null ? l.Trainer.FullName : "Atanmadı",
    //                StartTime = l.StartTime,
    //                Capacity = l.Capacity,
    //                AttendeeCount = l.Attendees!.Count
    //            }).ToListAsync(),

    //        // --- YENİ: DEVAMSIZLIK RAPORU (IsAttended Check) ---
    //        DailyAbsentees = await _context.LessonAttendees
    //            .AsNoTracking()
    //            .Include(a => a.User)
    //            .Include(a => a.Lesson)
    //                .ThenInclude(l => l.Trainer)
    //            .Where(a => a.Lesson!.StartTime.Date == today &&
    //                        a.Lesson.StartTime < now &&
    //                        !a.IsAttended && // Veritabanındaki yeni kolon
    //                        !a.IsDeleted)
    //            .OrderByDescending(a => a.Lesson.StartTime)
    //            .Select(a => new AbsenteeDto
    //            {
    //                MemberName = $"{a.User!.FirstName} {a.User.LastName}",
    //                LessonName = a.Lesson.Name,
    //                TrainerName = a.Lesson.Trainer != null ? a.Lesson.Trainer.FullName : "Eğitmen Belirtilmedi",
    //                LessonTime = a.Lesson.StartTime
    //            }).ToListAsync()
    //    };

    //    return dto;
    //}

    public async Task<DashboardSummaryDto> GetSummaryStatsAsync()
    {
        var today = DateTime.Today;
        var now = DateTime.Now;

        // Performans için Queryable bazlı alt yapı
        var subQuery = _context.Subscriptions.AsNoTracking();

        var dto = new DashboardSummaryDto
        {
            // --- CANLI SAYAÇLAR ---
            TotalMembers = await _context.Users.CountAsync(u => !u.IsDeleted),

            ActiveSubscriptions = await subQuery
                .CountAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate > now),

            TotalTrainers = await _context.Trainers.CountAsync(t => !t.IsDeleted),

            PendingSubscriptionCount = await subQuery
                .CountAsync(s => s.Status == SubscriptionStatus.Pending && !s.IsDeleted),

            TodayLessonsCount = await _context.Lessons
                .CountAsync(l => l.StartTime.Date == today && !l.IsDeleted),

            // --- SON KAYITLAR (Projection) ---
            RecentMembers = await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentMemberDto
                {
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                }).ToListAsync(),

            // --- BUGÜNÜN AKIŞI ---
            TodaysLessons = await _context.Lessons
                .AsNoTracking()
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
                    AttendeeCount = l.Attendees!.Count
                }).ToListAsync(),

            // --- DEVAMSIZLIK RAPORU (Yeni Tanımlanan DTO Kullanımı) ---
            DailyAbsentees = await _context.LessonAttendees
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Trainer)
                .Where(a => a.Lesson!.StartTime.Date == today &&
                            a.Lesson.StartTime < now &&
                            !a.IsAttended &&
                            !a.IsDeleted)
                .Select(a => new AbsenteeDto
                {
                    MemberName = $"{a.User!.FirstName} {a.User.LastName}",
                    LessonName = a.Lesson.Name,
                    TrainerName = a.Lesson.Trainer != null ? a.Lesson.Trainer.FullName : "Belirtilmemiş",
                    LessonTime = a.Lesson.StartTime
                }).ToListAsync()
        };

        return dto;
    }
}
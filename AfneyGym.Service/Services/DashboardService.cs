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

    public async Task TrackHeroVariantExposureAsync(string visitorId, string variant)
    {
        if (string.IsNullOrWhiteSpace(visitorId))
            return;

        var normalizedVariant = NormalizeVariant(variant);

        var existing = await _context.HeroVariantExposures
            .FirstOrDefaultAsync(x => x.VisitorId == visitorId && !x.IsDeleted);

        if (existing == null)
        {
            _context.HeroVariantExposures.Add(new HeroVariantExposure
            {
                VisitorId = visitorId,
                Variant = normalizedVariant
            });
        }
        else if (!string.Equals(existing.Variant, normalizedVariant, StringComparison.Ordinal))
        {
            existing.Variant = normalizedVariant;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            return;
        }

        await _context.SaveChangesAsync();
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
                    .ThenInclude(l => l!.Trainer)
                .Where(a => a.Lesson!.StartTime.Date == today &&
                            a.Lesson.StartTime < now &&
                            !a.IsAttended &&
                            !a.IsDeleted)
                .Select(a => new AbsenteeDto
                {
                    MemberName = a.User != null
                        ? $"{a.User.FirstName} {a.User.LastName}"
                        : "Belirtilmemiş",
                    LessonName = a.Lesson != null ? a.Lesson.Name : "Belirtilmemiş",
                    TrainerName = a.Lesson != null && a.Lesson.Trainer != null ? a.Lesson.Trainer.FullName : "Belirtilmemiş",
                    LessonTime = a.Lesson != null ? a.Lesson.StartTime : DateTime.MinValue
                }).ToListAsync()
        };

        return dto;
    }

    public async Task<LandingKpiDto> GetLandingKpisAsync()
    {
        var now = DateTime.Now;

        var activeMembers = await _context.Users
            .AsNoTracking()
            .CountAsync(u => !u.IsDeleted && u.Role == UserRole.Member);

        var activeSubscriptions = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => !s.IsDeleted && s.Status == SubscriptionStatus.Active && s.EndDate > now);

        var completedLessons = await _context.LessonAttendees
            .AsNoTracking()
            .CountAsync(a => !a.IsDeleted && a.IsAttended);

        var totalTrainers = await _context.Trainers
            .AsNoTracking()
            .CountAsync(t => !t.IsDeleted);

        return new LandingKpiDto
        {
            ActiveMembers = activeMembers,
            ActiveSubscriptions = activeSubscriptions,
            CompletedLessons = completedLessons,
            TotalTrainers = totalTrainers
        };
    }

    public async Task<AnalyticsDto> GetAnalyticsAsync()
    {
        var now = DateTime.Now;
        var last7Days = now.AddDays(-7);

        // Üyelik metrikler
        var totalMembers = await _context.Users
            .AsNoTracking()
            .CountAsync(u => !u.IsDeleted && u.Role == UserRole.Member);

        var activeSubscriptions = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => !s.IsDeleted && s.Status == SubscriptionStatus.Active && s.EndDate > now);

        var pendingApprovals = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => !s.IsDeleted && s.Status == SubscriptionStatus.Pending);

        // Ders metrikler
        var allLessons = await _context.Lessons
            .AsNoTracking()
            .CountAsync(l => !l.IsDeleted);

        var completedLessons = await _context.Lessons
            .AsNoTracking()
            .CountAsync(l => !l.IsDeleted && l.EndTime < now);

        var totalAttendees = await _context.LessonAttendees
            .AsNoTracking()
            .CountAsync(a => !a.IsDeleted);

        var attendedLessons = await _context.LessonAttendees
            .AsNoTracking()
            .CountAsync(a => !a.IsDeleted && a.IsAttended);

        var averageLessonAttendance = totalAttendees > 0 ? (decimal)attendedLessons / totalAttendees * 100 : 0;
        var noShowCount = totalAttendees - attendedLessons;

        // Ödeme metrikler
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var totalRevenue = payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Sum(p => p.Amount);

        var successfulPayments = payments.Count(p => p.Status == PaymentStatus.Completed);
        var failedPayments = payments.Count(p => p.Status == PaymentStatus.Failed);

        var activeSubPrice = activeSubscriptions > 0
            ? await _context.Subscriptions
                .AsNoTracking()
                .Where(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted)
                .AverageAsync(s => s.Price)
            : 0;

        // Eğitmen metrikler
        var totalTrainers = await _context.Trainers
            .AsNoTracking()
            .CountAsync(t => !t.IsDeleted);

        var activeTrainers = await _context.Lessons
            .AsNoTracking()
            .Select(l => l.TrainerId)
            .Distinct()
            .CountAsync();

        // Son 7 gün metrikleri
        var last7DaysMetrics = new List<DailyMetricDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i).Date;
            var nextDate = date.AddDays(1);

            var newSubs = await _context.Subscriptions
                .AsNoTracking()
                .CountAsync(s => s.CreatedAt >= date && s.CreatedAt < nextDate && !s.IsDeleted);

            var lessonsHeld = await _context.Lessons
                .AsNoTracking()
                .CountAsync(l => l.StartTime.Date == date && !l.IsDeleted);

            var dailyRevenue = await _context.Payments
                .AsNoTracking()
                .Where(p => p.CreatedAt >= date && p.CreatedAt < nextDate && p.Status == PaymentStatus.Completed && !p.IsDeleted)
                .SumAsync(p => p.Amount);

            var registrations = await _context.Users
                .AsNoTracking()
                .CountAsync(u => u.CreatedAt >= date && u.CreatedAt < nextDate && !u.IsDeleted && u.Role == UserRole.Member);

            last7DaysMetrics.Add(new DailyMetricDto
            {
                Date = date,
                NewSubscriptions = newSubs,
                LessonsHeld = lessonsHeld,
                DailyRevenue = dailyRevenue,
                Registrations = registrations
            });
        }

        // Renewal rate (son aylardaki yenilenen aidatlar)
        var renewedLast30Days = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => !s.IsDeleted && s.LastRenewalDate.HasValue && s.LastRenewalDate >= now.AddDays(-30));

        var renewalRate = activeSubscriptions > 0 ? (int)((decimal)renewedLast30Days / activeSubscriptions * 100) : 0;

        var heroVariantACount = await _context.HeroVariantExposures
            .AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.Variant == "a");

        var heroVariantBCount = await _context.HeroVariantExposures
            .AsNoTracking()
            .CountAsync(x => !x.IsDeleted && x.Variant == "b");

        return new AnalyticsDto
        {
            TotalMembers = totalMembers,
            ActiveSubscriptions = activeSubscriptions,
            PendingApprovals = pendingApprovals,
            SubscriptionRenewalRate = renewalRate,
            TotalLessons = allLessons,
            CompletedLessons = completedLessons,
            AverageLessonAttendance = averageLessonAttendance,
            NoShowCount = noShowCount,
            TotalRevenue = totalRevenue,
            SuccessfulPayments = successfulPayments,
            FailedPayments = failedPayments,
            AverageSubscriptionPrice = (decimal)activeSubPrice,
            TotalTrainers = totalTrainers,
            ActiveTrainers = activeTrainers,
            HeroVariantACount = heroVariantACount,
            HeroVariantBCount = heroVariantBCount,
            Last7DaysMetrics = last7DaysMetrics
        };
    }

    private static string NormalizeVariant(string? variant)
    {
        return string.Equals(variant, "b", StringComparison.OrdinalIgnoreCase) ? "b" : "a";
    }
}
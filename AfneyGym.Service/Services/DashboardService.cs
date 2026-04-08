using AfneyGym.Common.DTOs;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities; // UserRole enum'ı için eklendi
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

    public async Task<DashboardSummaryDto> GetSummaryStatsAsync()
    {
        // MÜHENDİSLİK DÜZELTMESİ: Sadece 'Member' rolündeki kullanıcılar sayılmalı.
        // Yönetici ve Personel, 'Üye' istatistiğine dahil edilmez.
        var totalRealMembers = await _context.Users
            .CountAsync(u => !u.IsDeleted && u.Role == UserRole.Member);

        var activeSubscriptions = await _context.Subscriptions
            .CountAsync(s => s.EndDate > DateTime.Now && !s.IsDeleted);

        var totalTrainers = await _context.Trainers
            .CountAsync(t => !t.IsDeleted);

        var todayLessons = await _context.Lessons
            .CountAsync(l => l.StartTime.Date == DateTime.Today && !l.IsDeleted);

        return new DashboardSummaryDto
        {
            TotalMembers = totalRealMembers, // Artık sadece gerçek üyeler
            ActiveSubscriptions = activeSubscriptions,
            TotalTrainers = totalTrainers,
            TodayLessonsCount = todayLessons
        };
    }
}
using AfneyGym.Data.Context;
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

    public async Task<Dictionary<string, int>> GetSummaryStatsAsync()
    {
        return new Dictionary<string, int>
        {
            { "TotalMembers", await _context.Users.CountAsync(u => !u.IsDeleted) },
            { "ActiveSubscriptions", await _context.Subscriptions.CountAsync(s => s.Status == Domain.Entities.SubscriptionStatus.Active) },
            { "TotalTrainers", await _context.Trainers.CountAsync() },
            { "TodayLessons", await _context.Lessons.CountAsync(l => l.StartTime.Date == DateTime.Today) }
        };
    }
}
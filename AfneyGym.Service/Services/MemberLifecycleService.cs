using AfneyGym.Common.DTOs;
using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.Service.Services;

public class MemberLifecycleService : IMemberLifecycleService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public MemberLifecycleService(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> CheckInAsync(Guid userId, Guid? gymId = null, string method = "Manual")
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted);
        if (!userExists)
            return false;

        var activeSession = await _context.GymCheckIns
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted && c.CheckOutTime == null);

        if (activeSession != null)
            return false;

        _context.GymCheckIns.Add(new GymCheckIn
        {
            UserId = userId,
            GymId = gymId,
            CheckInTime = DateTime.Now,
            CheckInMethod = string.IsNullOrWhiteSpace(method) ? "Manual" : method.Trim()
        });

        await _context.SaveChangesAsync();
        await UpdateLifecycleStatusAsync(userId);
        return true;
    }

    public async Task<bool> CheckOutAsync(Guid userId)
    {
        var activeSession = await _context.GymCheckIns
            .Where(c => c.UserId == userId && !c.IsDeleted && c.CheckOutTime == null)
            .OrderByDescending(c => c.CheckInTime)
            .FirstOrDefaultAsync();

        if (activeSession == null)
            return false;

        activeSession.CheckOutTime = DateTime.Now;
        activeSession.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetMonthlyCheckInCountAsync(Guid userId, DateTime? referenceDate = null)
    {
        var baseDate = referenceDate ?? DateTime.Now;
        var monthStart = new DateTime(baseDate.Year, baseDate.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        return await _context.GymCheckIns
            .CountAsync(c => c.UserId == userId && !c.IsDeleted && c.CheckInTime >= monthStart && c.CheckInTime < nextMonth);
    }

    public async Task<List<CheckInDto>> GetRecentCheckInsAsync(Guid userId, int days = 30)
    {
        var since = DateTime.Now.AddDays(-Math.Max(1, days));
        return await _context.GymCheckIns
            .Where(c => c.UserId == userId && !c.IsDeleted && c.CheckInTime >= since)
            .OrderByDescending(c => c.CheckInTime)
            .Select(c => new CheckInDto
            {
                CheckInTime = c.CheckInTime,
                CheckOutTime = c.CheckOutTime,
                DurationMinutes = c.CheckOutTime.HasValue ? (int?)EF.Functions.DateDiffMinute(c.CheckInTime, c.CheckOutTime.Value) : null,
                CheckInMethod = c.CheckInMethod
            })
            .ToListAsync();
    }

    public async Task<MemberLifecycleStage> UpdateLifecycleStatusAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscriptions)
            .Include(u => u.Attendees)
            .Include(u => u.CheckIns)
            .Include(u => u.LifecycleStatus)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
            return MemberLifecycleStage.Churned;

        var now = DateTime.Now;
        var hasActiveSubscription = user.Subscriptions.Any(s => !s.IsDeleted && s.Status == SubscriptionStatus.Active && s.EndDate > now);
        var lastAttendance = user.Attendees
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => (DateTime?)a.CreatedAt)
            .FirstOrDefault();
        var lastCheckIn = user.CheckIns
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CheckInTime)
            .Select(c => (DateTime?)c.CheckInTime)
            .FirstOrDefault();

        var lastActivity = MaxDate(lastAttendance, lastCheckIn) ?? user.CreatedAt;
        var daysInactive = (int)(now - lastActivity).TotalDays;

        var newStage = ResolveStage(user.CreatedAt, hasActiveSubscription, daysInactive);
        var riskScore = CalculateRiskScore(hasActiveSubscription, daysInactive);

        if (user.LifecycleStatus == null)
        {
            user.LifecycleStatus = new UserLifecycleStatus
            {
                UserId = user.Id,
                CurrentStage = newStage,
                TransitionDate = now,
                TransitionReason = BuildTransitionReason(newStage, hasActiveSubscription, daysInactive),
                RiskScore = riskScore
            };
            _context.UserLifecycleStatuses.Add(user.LifecycleStatus);
        }
        else
        {
            if (user.LifecycleStatus.CurrentStage != newStage)
            {
                user.LifecycleStatus.CurrentStage = newStage;
                user.LifecycleStatus.TransitionDate = now;
                user.LifecycleStatus.TransitionReason = BuildTransitionReason(newStage, hasActiveSubscription, daysInactive);
                if (newStage == MemberLifecycleStage.Active)
                    user.LifecycleStatus.ReactivatedAt = now;
            }

            user.LifecycleStatus.RiskScore = riskScore;
            user.LifecycleStatus.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return newStage;
    }

    public async Task<MemberLifecycleStage> GetCurrentStageAsync(Guid userId)
    {
        var status = await _context.UserLifecycleStatuses
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .Select(s => (MemberLifecycleStage?)s.CurrentStage)
            .FirstOrDefaultAsync();

        if (status.HasValue)
            return status.Value;

        return await UpdateLifecycleStatusAsync(userId);
    }

    public async Task<List<Guid>> GetAtRiskMembersAsync()
    {
        return await _context.UserLifecycleStatuses
            .Where(s => !s.IsDeleted && s.CurrentStage == MemberLifecycleStage.AtRisk)
            .Select(s => s.UserId)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetChurnRiskMembersAsync()
    {
        var now = DateTime.Now;
        return await _context.Subscriptions
            .Where(s => !s.IsDeleted
                        && s.Status == SubscriptionStatus.Active
                        && s.EndDate > now
                        && s.EndDate <= now.AddDays(7))
            .Select(s => s.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<int> UpdateAllMembersLifecycleAsync()
    {
        var userIds = await _context.Users
            .Where(u => !u.IsDeleted && u.Role == Common.Enums.UserRole.Member)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var userId in userIds)
            await UpdateLifecycleStatusAsync(userId);

        return userIds.Count;
    }

    public async Task AddBodyMetricAsync(Guid userId, BodyMetricCreateDto metric)
    {
        var bodyMetric = new UserBodyMetric
        {
            UserId = userId,
            Weight = metric.Weight,
            BodyFatPercentage = metric.BodyFatPercentage,
            MuscleMass = metric.MuscleMass,
            BMI = metric.BMI,
            ChestCircumference = metric.ChestCircumference,
            WaistCircumference = metric.WaistCircumference,
            HipCircumference = metric.HipCircumference,
            ArmCircumference = metric.ArmCircumference,
            Notes = metric.Notes,
            MeasurementDate = metric.MeasurementDate == default ? DateTime.UtcNow : metric.MeasurementDate
        };

        _context.UserBodyMetrics.Add(bodyMetric);
        await _context.SaveChangesAsync();
    }

    public async Task<List<UserBodyMetric>> GetBodyMetricsAsync(Guid userId, int take = 12)
    {
        return await _context.UserBodyMetrics
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .OrderByDescending(m => m.MeasurementDate)
            .Take(Math.Max(1, take))
            .ToListAsync();
    }

    public async Task<BodyMetricSummaryDto?> GetLatestBodyMetricSummaryAsync(Guid userId, int months = 1)
    {
        var latest = await _context.UserBodyMetrics
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .OrderByDescending(m => m.MeasurementDate)
            .FirstOrDefaultAsync();

        if (latest == null)
            return null;

        var referenceDate = latest.MeasurementDate.AddMonths(-Math.Max(1, months));
        var historical = await _context.UserBodyMetrics
            .Where(m => m.UserId == userId && !m.IsDeleted && m.MeasurementDate <= referenceDate)
            .OrderByDescending(m => m.MeasurementDate)
            .FirstOrDefaultAsync();

        return new BodyMetricSummaryDto
        {
            Weight = latest.Weight,
            BodyFatPercentage = latest.BodyFatPercentage,
            WeightChangeMonth = historical == null ? 0 : latest.Weight - historical.Weight,
            BodyFatChangeMonth = historical == null || !latest.BodyFatPercentage.HasValue || !historical.BodyFatPercentage.HasValue
                ? null
                : latest.BodyFatPercentage.Value - historical.BodyFatPercentage.Value,
            MeasurementDate = latest.MeasurementDate
        };
    }

    public async Task AddGoalAsync(Guid userId, UserGoalCreateDto goal)
    {
        _context.UserGoals.Add(new UserGoal
        {
            UserId = userId,
            Title = goal.Title,
            Description = goal.Description,
            GoalType = goal.GoalType,
            StartValue = goal.StartValue,
            TargetValue = goal.TargetValue,
            CurrentValue = goal.StartValue,
            TargetDate = goal.TargetDate,
            Status = GoalStatus.Active
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<UserGoal>> GetActiveGoalsAsync(Guid userId)
    {
        return await _context.UserGoals
            .Where(g => g.UserId == userId && !g.IsDeleted && g.Status == GoalStatus.Active)
            .OrderBy(g => g.TargetDate)
            .ToListAsync();
    }

    public async Task<bool> UpdateGoalProgressAsync(Guid goalId, decimal currentValue)
    {
        var goal = await _context.UserGoals.FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);
        if (goal == null)
            return false;

        goal.CurrentValue = currentValue;
        goal.UpdatedAt = DateTime.Now;

        if ((goal.TargetValue >= goal.StartValue && goal.CurrentValue >= goal.TargetValue)
            || (goal.TargetValue < goal.StartValue && goal.CurrentValue <= goal.TargetValue))
        {
            goal.Status = GoalStatus.Completed;
            goal.CompletedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteGoalAsync(Guid goalId)
    {
        var goal = await _context.UserGoals.FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);
        if (goal == null)
            return false;

        goal.Status = GoalStatus.Completed;
        goal.CompletedAt = DateTime.Now;
        goal.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SendAtRiskRemindersAsync()
    {
        var threshold = DateTime.Now.AddDays(-7);
        var atRiskMembers = await _context.UserLifecycleStatuses
            .Include(s => s.User)
            .Where(s => !s.IsDeleted
                        && s.CurrentStage == MemberLifecycleStage.AtRisk
                        && s.User != null
                        && !s.User.IsDeleted
                        && (s.LastReminderDate == null || s.LastReminderDate < threshold))
            .ToListAsync();

        foreach (var status in atRiskMembers)
        {
            var user = status.User;
            if (user == null)
                continue;

            await _emailService.SendEmailAsync(
                user.Email,
                "Seni salonda tekrar gormek istiyoruz",
                $"Merhaba {user.FirstName}, son zamanlarda salonu ozledin gibi gorunuyor. Yeni ders programini kontrol et ve geri don!");

            status.ReminderEmailSent = true;
            status.LastReminderDate = DateTime.Now;
            status.UpdatedAt = DateTime.Now;
        }

        if (atRiskMembers.Count > 0)
            await _context.SaveChangesAsync();
    }

    public async Task SendRenewalRemindersAsync()
    {
        var now = DateTime.Now;
        var soon = now.AddDays(7);

        var expiringSubs = await _context.Subscriptions
            .Include(s => s.User)
            .Where(s => !s.IsDeleted
                        && s.Status == SubscriptionStatus.Active
                        && s.EndDate > now
                        && s.EndDate <= soon
                        && s.User != null
                        && !s.User.IsDeleted)
            .ToListAsync();

        foreach (var sub in expiringSubs)
        {
            var user = sub.User;
            if (user == null)
                continue;

            await _emailService.SendEmailAsync(
                user.Email,
                "Uyeligin yakinda sona eriyor",
                $"Merhaba {user.FirstName}, uyeligin {sub.EndDate:dd.MM.yyyy} tarihinde sona eriyor. Yenileyerek derslerine ara vermeden devam edebilirsin.");
        }
    }

    private static MemberLifecycleStage ResolveStage(DateTime userCreatedAt, bool hasActiveSubscription, int daysInactive)
    {
        if (!hasActiveSubscription)
            return MemberLifecycleStage.Churned;

        if ((DateTime.Now - userCreatedAt).TotalDays <= 30)
            return MemberLifecycleStage.NewMember;

        if (daysInactive >= 45)
            return MemberLifecycleStage.Inactive;

        if (daysInactive >= 21)
            return MemberLifecycleStage.AtRisk;

        return MemberLifecycleStage.Active;
    }

    private static int CalculateRiskScore(bool hasActiveSubscription, int daysInactive)
    {
        if (!hasActiveSubscription)
            return 100;

        var baseScore = Math.Min(90, daysInactive * 3);
        return Math.Max(0, baseScore);
    }

    private static string BuildTransitionReason(MemberLifecycleStage stage, bool hasActiveSubscription, int daysInactive)
    {
        return stage switch
        {
            MemberLifecycleStage.Churned when !hasActiveSubscription => "Aktif abonelik bulunamadi",
            MemberLifecycleStage.NewMember => "Ilk 30 gun onboarding asamasi",
            MemberLifecycleStage.AtRisk => $"Son aktiviteden bu yana {daysInactive} gun gecti",
            MemberLifecycleStage.Inactive => $"Uzun sure aktivite yok ({daysInactive} gun)",
            _ => "Duzenli aktivite ve aktif abonelik"
        };
    }

    private static DateTime? MaxDate(DateTime? a, DateTime? b)
    {
        if (!a.HasValue) return b;
        if (!b.HasValue) return a;
        return a.Value >= b.Value ? a : b;
    }
}


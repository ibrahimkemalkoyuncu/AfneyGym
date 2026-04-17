using AfneyGym.Common.DTOs;
using AfneyGym.Domain.Entities;

namespace AfneyGym.Domain.Interfaces;

public interface IMemberLifecycleService
{
    Task<bool> CheckInAsync(Guid userId, Guid? gymId = null, string method = "Manual");
    Task<bool> CheckOutAsync(Guid userId);
    Task<int> GetMonthlyCheckInCountAsync(Guid userId, DateTime? referenceDate = null);
    Task<List<CheckInDto>> GetRecentCheckInsAsync(Guid userId, int days = 30);

    Task<MemberLifecycleStage> UpdateLifecycleStatusAsync(Guid userId);
    Task<MemberLifecycleStage> GetCurrentStageAsync(Guid userId);
    Task<List<Guid>> GetAtRiskMembersAsync();
    Task<List<Guid>> GetChurnRiskMembersAsync();
    Task<int> UpdateAllMembersLifecycleAsync();

    Task AddBodyMetricAsync(Guid userId, BodyMetricCreateDto metric);
    Task<List<UserBodyMetric>> GetBodyMetricsAsync(Guid userId, int take = 12);
    Task<BodyMetricSummaryDto?> GetLatestBodyMetricSummaryAsync(Guid userId, int months = 1);

    Task AddGoalAsync(Guid userId, UserGoalCreateDto goal);
    Task<List<UserGoal>> GetActiveGoalsAsync(Guid userId);
    Task<bool> UpdateGoalProgressAsync(Guid goalId, decimal currentValue);
    Task<bool> CompleteGoalAsync(Guid goalId);

    Task SendAtRiskRemindersAsync();
    Task SendRenewalRemindersAsync();
}


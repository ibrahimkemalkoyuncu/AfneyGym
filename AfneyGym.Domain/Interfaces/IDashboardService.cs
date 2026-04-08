namespace AfneyGym.Domain.Interfaces;

public interface IDashboardService
{
    Task<Dictionary<string, int>> GetSummaryStatsAsync();
}
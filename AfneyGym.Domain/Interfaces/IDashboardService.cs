using AfneyGym.Common.DTOs;

namespace AfneyGym.Domain.Interfaces;

public interface IDashboardService
{
    // CS0738 Çözümü: Sözleşmeyi DTO dönecek şekilde güncelledik.
    Task<DashboardSummaryDto> GetSummaryStatsAsync();
    Task<LandingKpiDto> GetLandingKpisAsync();
    Task<AnalyticsDto> GetAnalyticsAsync();
    Task TrackHeroVariantExposureAsync(string visitorId, string variant);
}
namespace AfneyGym.Common.DTOs;

public class MemberLifecycleDto
{
    public Guid UserId { get; set; }
    public string CurrentStage { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public int MonthlyCheckInCount { get; set; }
    public decimal? LatestWeight { get; set; }
    public int ActiveGoalCount { get; set; }
    public DateTime TransitionDate { get; set; }
}

public class CheckInDto
{
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string CheckInMethod { get; set; } = "Manual";
}

public class BodyMetricSummaryDto
{
    public decimal Weight { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal WeightChangeMonth { get; set; }
    public decimal? BodyFatChangeMonth { get; set; }
    public DateTime MeasurementDate { get; set; }
}

public class BodyMetricCreateDto
{
    public decimal Weight { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal? MuscleMass { get; set; }
    public decimal? BMI { get; set; }
    public decimal? ChestCircumference { get; set; }
    public decimal? WaistCircumference { get; set; }
    public decimal? HipCircumference { get; set; }
    public decimal? ArmCircumference { get; set; }
    public string? Notes { get; set; }
    public DateTime MeasurementDate { get; set; } = DateTime.UtcNow;
}

public class UserGoalCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string GoalType { get; set; } = "Weight";
    public decimal StartValue { get; set; }
    public decimal TargetValue { get; set; }
    public DateTime TargetDate { get; set; }
}


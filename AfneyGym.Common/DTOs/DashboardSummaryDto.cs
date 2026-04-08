namespace AfneyGym.Common.DTOs;

public class DashboardSummaryDto
{
    public int TotalMembers { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TotalTrainers { get; set; }
    public int TodayLessonsCount { get; set; }

    // Entity yerine DTO listeleri kullanılarak CS0246 hataları çözüldü
    public List<RecentMemberDto> RecentMembers { get; set; } = new();
    public List<TodaysLessonDto> TodaysLessons { get; set; } = new();
}

// Tablolar için hafif veri yapıları
public class RecentMemberDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TodaysLessonDto
{
    public string Name { get; set; } = string.Empty;
    public string TrainerName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int AttendeeCount { get; set; }
    public int Capacity { get; set; }
}
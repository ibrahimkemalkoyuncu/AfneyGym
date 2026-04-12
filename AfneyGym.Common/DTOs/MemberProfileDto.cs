namespace AfneyGym.Common.DTOs;

public class MemberProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }

    // İstatistikler
    public int TotalRegisteredLessons { get; set; }
    public int AttendedLessonsCount { get; set; }
    public double AttendanceRate => TotalRegisteredLessons > 0
        ? Math.Round((double)AttendedLessonsCount / TotalRegisteredLessons * 100, 1)
        : 0;

    // Geçmiş Veriler
    public List<MemberAttendanceHistoryDto> AttendanceHistory { get; set; } = new();
}

public class MemberAttendanceHistoryDto
{
    public string LessonName { get; set; } = string.Empty;
    public DateTime LessonDate { get; set; }
    public string TrainerName { get; set; } = string.Empty;
    public bool IsAttended { get; set; }
}
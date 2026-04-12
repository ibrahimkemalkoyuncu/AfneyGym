namespace AfneyGym.Common.DTOs;

public class UserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string GymName { get; set; } = string.Empty;
    public int TotalAttendedLessons { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }

    // Üyenin katıldığı derslerin özeti
    public List<UserLessonHistoryDto> AttendedLessons { get; set; } = new();
}

public class UserLessonHistoryDto
{
    public string LessonName { get; set; } = string.Empty;
    public string TrainerName { get; set; } = string.Empty;
    public DateTime LessonDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
}
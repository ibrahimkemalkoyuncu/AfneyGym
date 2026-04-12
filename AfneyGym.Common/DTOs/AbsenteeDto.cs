namespace AfneyGym.Common.DTOs;

public record AbsenteeDto
{
    public string MemberName { get; init; } = string.Empty;
    public string LessonName { get; init; } = string.Empty;
    public string TrainerName { get; init; } = string.Empty;
    public DateTime LessonTime { get; init; }
}
namespace AfneyGym.Domain.Entities;

public class LessonAttendee : BaseEntity
{
    public Guid LessonId { get; set; }
    public virtual Lesson? Lesson { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}
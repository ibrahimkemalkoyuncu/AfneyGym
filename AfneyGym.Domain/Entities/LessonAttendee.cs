namespace AfneyGym.Domain.Entities;

public class LessonAttendee : BaseEntity
{
    public Guid LessonId { get; set; }
    public virtual Lesson? Lesson { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // YOKLAMA SİSTEMİ İÇİN KRİTİK ALAN
    public bool IsAttended { get; set; } = false;

    public DateTime? ReminderSentAt { get; set; }
}
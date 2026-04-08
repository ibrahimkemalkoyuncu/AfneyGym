using AfneyGym.Domain.Entities;

namespace AfneyGym.Domain.Entities;

public class Lesson : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }

    public Guid TrainerId { get; set; }
    public virtual Trainer? Trainer { get; set; }

    public Guid GymId { get; set; }
    public virtual Gym? Gym { get; set; }

    // KRİTİK EKSİK BURASI: 
    // Bu satır olmazsa Controller'daki .Include(l => l.Attendees) hiçbir veri getirmez.
    public virtual ICollection<LessonAttendee> Attendees { get; set; } = new List<LessonAttendee>();
}
/* AÇIKLAMA: EF Core'un Lesson tablosundan LessonAttendees tablosuna 
   JOIN atabilmesi için bu koleksiyon tanımı zorunludur. */
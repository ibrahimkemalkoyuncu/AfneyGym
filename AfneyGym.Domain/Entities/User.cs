using System.ComponentModel.DataAnnotations;

namespace AfneyGym.Domain.Entities;

// CS0103 Düzeltmesi: UserService içinde kullanılan UserRole yapısı burada tanımlı olmalıdır.
public enum UserRole
{
    Admin = 0,
    Staff = 1,
    Member = 2
}

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Tip güvenliği için Enum kullanımı
    public UserRole Role { get; set; } = UserRole.Member;

    public Guid? GymId { get; set; }
    public virtual Gym? Gym { get; set; }

    // --- CS1061 Düzeltmesi ---
    // UserService içinde .Include(u => u.Subscriptions) çağrısının çalışması için bu gereklidir.
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    // İlişkisel bütünlük için katılımcı listesi
    public virtual ICollection<LessonAttendee> Attendees { get; set; } = new List<LessonAttendee>();
}
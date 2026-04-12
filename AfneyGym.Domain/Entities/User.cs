using AfneyGym.Common.Enums; // KRİTİK: Yeni enum katmanını bağladık
using System.ComponentModel.DataAnnotations;

namespace AfneyGym.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Artik bu property AfneyGym.Common.Enums.UserRole tipindedir
    public UserRole Role { get; set; } = UserRole.Member;

    public Guid? GymId { get; set; }
    public virtual Gym? Gym { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<LessonAttendee> Attendees { get; set; } = new List<LessonAttendee>();
}
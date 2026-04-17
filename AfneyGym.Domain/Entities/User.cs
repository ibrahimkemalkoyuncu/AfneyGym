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
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public virtual ICollection<LessonAttendee> Attendees { get; set; } = new List<LessonAttendee>();

    // --- PHASE 1: ÜYE LİFESİKLE TAKIBI ---
    public virtual ICollection<UserBodyMetric> BodyMetrics { get; set; } = new List<UserBodyMetric>();
    public virtual ICollection<UserGoal> Goals { get; set; } = new List<UserGoal>();
    public virtual ICollection<GymCheckIn> CheckIns { get; set; } = new List<GymCheckIn>();
    public virtual UserLifecycleStatus? LifecycleStatus { get; set; }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace AfneyGym.Domain.Entities;

/// <summary>
/// Üyenin hedeflerini ve ilerleme durumunu takip eder.
/// Örn: "60 kg'a ulaş", "5 derse/ay katıl", "6 pack karın" vb.
/// </summary>
public class UserGoal : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Hedefin başlığı (Örn: "Kilo vermek", "Kas kazanmak")
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Detaylı açıklama
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Hedef tipi: Weight, MuscleMass, BodyFat, LessonAttendance, etc.
    /// </summary>
    public string GoalType { get; set; } = null!; // Örn: "Weight", "Attendance"

    /// <summary>
    /// Başlangıç değeri
    /// </summary>
    public decimal StartValue { get; set; }

    /// <summary>
    /// Hedef değeri
    /// </summary>
    public decimal TargetValue { get; set; }

    /// <summary>
    /// Mevcut ilerleme (en son güncellenen değer)
    /// </summary>
    public decimal CurrentValue { get; set; }

    /// <summary>
    /// İlerleme yüzdesi (0-100)
    /// </summary>
    [NotMapped]
    public decimal ProgressPercentage => TargetValue != StartValue
        ? Math.Round(((CurrentValue - StartValue) / (TargetValue - StartValue)) * 100, 1)
        : 0;

    /// <summary>
    /// Hedef tarihi
    /// </summary>
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Tamamlanma tarihi (null = henüz tamamlanmadı)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Hedef durumu: Active, Completed, Abandoned
    /// </summary>
    public GoalStatus Status { get; set; } = GoalStatus.Active;
}

/// <summary>
/// Hedef durumu enum'ı
/// </summary>
public enum GoalStatus
{
    Active = 0,
    Completed = 1,
    Abandoned = 2
}


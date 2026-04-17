using System.ComponentModel.DataAnnotations.Schema;

namespace AfneyGym.Domain.Entities;

/// <summary>
/// Üyenin gym'e her giriş-çıkışını kaydeder.
/// Check-in sistemi ile üyelerin düzenli olup olmadığını takip edebiliriz.
/// </summary>
public class GymCheckIn : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? GymId { get; set; }
    // public Gym? Gym { get; set; }

    /// <summary>
    /// Check-in zamanı (giriş)
    /// </summary>
    public DateTime CheckInTime { get; set; }

    /// <summary>
    /// Check-out zamanı (çıkış)
    /// </summary>
    public DateTime? CheckOutTime { get; set; }

    /// <summary>
    /// Gym'de geçirilen dakika
    /// </summary>
    [NotMapped]
    public int? DurationMinutes => CheckOutTime.HasValue
        ? (int)(CheckOutTime.Value - CheckInTime).TotalMinutes
        : null;

    /// <summary>
    /// Check-in yöntemi: Mobile, QRCode, Manual
    /// </summary>
    public string CheckInMethod { get; set; } = "Manual"; // Default

    /// <summary>
    /// Admin tarafından eklenen notlar (eksik işaret vb.)
    /// </summary>
    public string? Notes { get; set; }
}


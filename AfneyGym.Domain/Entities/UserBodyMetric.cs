
namespace AfneyGym.Domain.Entities;

/// <summary>
/// Üyenin vücut ölçümlerini takip eder.
/// Zaman içinde vücut değişimini görebilir ve hedefine karsı ilerleme ölçülebilir.
/// </summary>
public class UserBodyMetric : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Kilo (kg)
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Vücut yağ oranı (%)
    /// </summary>
    public decimal? BodyFatPercentage { get; set; }

    /// <summary>
    /// Kas kütlesi (kg)
    /// </summary>
    public decimal? MuscleMass { get; set; }

    /// <summary>
    /// BMI (Body Mass Index) - otomatik hesapla
    /// </summary>
    public decimal? BMI { get; set; }

    /// <summary>
    /// Göğüs çevresi (cm)
    /// </summary>
    public decimal? ChestCircumference { get; set; }

    /// <summary>
    /// Bel çevresi (cm)
    /// </summary>
    public decimal? WaistCircumference { get; set; }

    /// <summary>
    /// Kalça çevresi (cm)
    /// </summary>
    public decimal? HipCircumference { get; set; }

    /// <summary>
    /// Kol çevresi (cm)
    /// </summary>
    public decimal? ArmCircumference { get; set; }

    /// <summary>
    /// Admin tarafından eklenen notlar
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Ölçüm tarihi
    /// </summary>
    public DateTime MeasurementDate { get; set; } = DateTime.UtcNow;
}


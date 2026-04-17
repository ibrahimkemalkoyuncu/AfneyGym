
namespace AfneyGym.Domain.Entities;

/// <summary>
/// Üyenin yaşam döngüsü durumunu ve geçişini takip eder.
/// Her duruma geçişi kayıt altına alır ve sebepini belirleriz.
/// 
/// Durum akışı:
/// NewMember → Active → AtRisk → Inactive → Churned
/// veya Active → Churned (direkt iptal)
/// </summary>
public class UserLifecycleStatus : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// Mevcut durum
    /// </summary>
    public MemberLifecycleStage CurrentStage { get; set; } = MemberLifecycleStage.NewMember;

    /// <summary>
    /// Bu duruma geçiş tarihi
    /// </summary>
    public DateTime TransitionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Durum değişim nedeni
    /// Örn: "30+ gün ders katılmamış", "İptal talebi", "Abonelik süresi doldu"
    /// </summary>
    public string? TransitionReason { get; set; }

    /// <summary>
    /// Risk skoru (0-100): AtRisk durumunda hangi seviyede
    /// 80+ = Acil müdahale
    /// </summary>
    public int? RiskScore { get; set; }

    /// <summary>
    /// Geri kazanım tarihi (Churned → Active dönüşümü)
    /// </summary>
    public DateTime? ReactivatedAt { get; set; }

    /// <summary>
    /// Kişiye gönderilen hatırlatma e-maili mi?
    /// </summary>
    public bool ReminderEmailSent { get; set; } = false;

    /// <summary>
    /// Son hatırlatma e-maili tarihi
    /// </summary>
    public DateTime? LastReminderDate { get; set; }
}

/// <summary>
/// Üye yaşam döngüsü aşamaları
/// </summary>
public enum MemberLifecycleStage
{
    /// <summary>
    /// Yeni üye (ilk 30 gün)
    /// </summary>
    NewMember = 0,

    /// <summary>
    /// Aktif üye (düzenli ders katılıyor)
    /// </summary>
    Active = 1,

    /// <summary>
    /// Risk altında (pasifleşmeye başladı, müdahale gerekli)
    /// </summary>
    AtRisk = 2,

    /// <summary>
    /// Inaktif (30+ gün katılmamış, abonelik bitecek)
    /// </summary>
    Inactive = 3,

    /// <summary>
    /// Churn (Üyelik iptal edildi/süresi doldu)
    /// </summary>
    Churned = 4
}


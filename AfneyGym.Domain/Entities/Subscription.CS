using System;

namespace AfneyGym.Domain.Entities;

public class Subscription : BaseEntity
{
    // Hangi kullanıcıya ait olduğu (Foreign Key)
    public Guid UserId { get; set; }
    
    // Navigation Property: EF Core için kullanıcı ilişkisi
    public virtual User? User { get; set; }

    // Üyelik Paketi İsmi (Örn: Gold, Standart, 3 Aylık)
    public required string PlanName { get; set; }

    // Üyelik Başlangıç ve Bitiş
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Üyelik Ücreti (Sıfır NuGet kuralı: Money tipi için decimal kullanıyoruz)
    public decimal Price { get; set; }

    // Üyelik Durumu
    public SubscriptionStatus Status { get; set; }

    // Teknik Açıklama: Üyeliğin dondurulup dondurulmadığı kontrolü
    public bool IsFrozen { get; set; }
    public DateTime? FreezeDate { get; set; }
}


public enum SubscriptionStatus
{
    Pending = 1,    // Ödeme/Onay bekliyor
    Active = 2,     // Onaylanmış ve süresi devam ediyor
    Rejected = 3,   // Ödeme teyit edilemedi, reddedildi
    Expired = 4,    // Süresi dolmuş
    Canceled = 5    // Kullanıcı veya Admin tarafından iptal edildi
}
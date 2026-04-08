namespace AfneyGym.Common.DTOs;

public class DashboardSummaryDto
{
    // Üye sayısı (Users tablosundan)
    public int TotalMembers { get; set; }

    // Aktif abonelik sayısı (Subscriptions tablosundan)
    public int ActiveSubscriptions { get; set; }

    // Kayıtlı eğitmen sayısı (Trainers tablosundan)
    public int TotalTrainers { get; set; }

    // Sadece bugüne ait ders sayısı (Lessons tablosundan)
    public int TodayLessonsCount { get; set; }
}
/* MÜHENDİSLİK NOTU: 
   Bu sınıfın 'Common' katmanında olması, hem 'Service' hem de 'WebMvc' 
   katmanlarının bu veri yapısını tanımasını sağlar. */
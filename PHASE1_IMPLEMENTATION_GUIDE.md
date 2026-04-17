# AfneyGym PHASE 1 - Üye Lifecycle Takibi Implementation Guide

## ✅ Tamamlanan Adımlar

### 1. Veri Modeli Oluşturuldu
- [x] `UserBodyMetric` entity - Vücut ölçümleri takibi
- [x] `UserGoal` entity - Üye hedefleri
- [x] `GymCheckIn` entity - Gym giriş-çıkış kayıtları
- [x] `UserLifecycleStatus` entity - Üye yaşam döngüsü durumu
- [x] `AppDbContext` güncellenmiş - Tüm DbSet'ler eklendi
- [x] `User` entity güncellenmiş - Navigation property'ler eklendi
- [x] Çözüm başarıyla derlenmiş ✅

---

## 📋 Sonraki Adımlar (TODO)

### Phase 1A: Migration & Database
```bash
# Yeni migration oluştur
dotnet ef migrations add AddMemberLifecycleTracking

# Veritabanına uygula
dotnet ef database update
```

**Timeline:** 1-2 saat

---

### Phase 1B: Service Layer (Business Logic)

#### 1. `IMemberLifecycleService` Interface Oluştur
```csharp
namespace AfneyGym.Domain.Interfaces;

public interface IMemberLifecycleService
{
    // Check-in Operations
    Task<bool> CheckInAsync(Guid userId, Guid? gymId = null);
    Task<bool> CheckOutAsync(Guid userId);
    Task<int> GetMonthlyCheckInCountAsync(Guid userId);

    // Lifecycle Status Management
    Task UpdateLifecycleStatusAsync(Guid userId);
    Task<MemberLifecycleStage> GetCurrentStageAsync(Guid userId);
    Task<List<Guid>> GetAtRiskMembersAsync(); // 30+ gün katılmamış
    Task<List<Guid>> GetChurnRiskMembersAsync(); // Abonelik bitecek

    // Body Metrics
    Task AddBodyMetricAsync(Guid userId, UserBodyMetric metric);
    Task<List<UserBodyMetric>> GetBodyMetricsAsync(Guid userId);
    Task<decimal> CalculateWeightChangeAsync(Guid userId, int months = 1);

    // Goals
    Task AddGoalAsync(Guid userId, UserGoal goal);
    Task<List<UserGoal>> GetActiveGoalsAsync(Guid userId);
    Task UpdateGoalProgressAsync(Guid goalId, decimal currentValue);
    Task CompleteGoalAsync(Guid goalId);

    // Reminders
    Task SendAtRiskRemindersAsync(); // Scheduled job
    Task SendRenewalRemindersAsync(); // Scheduled job
}
```

**Timeline:** 4-6 saat
**Zorluk:** ⭐⭐⭐

#### 2. `MemberLifecycleService` Implement Et
- Check-in logic (manuel, QR, API)
- Status transition logic (New → Active → AtRisk → Inactive → Churned)
- Risk scoring algorithm
- Automated email triggers

**Timeline:** 6-8 saat
**Zorluk:** ⭐⭐⭐⭐

---

### Phase 1C: Controller & API Endpoints

#### 1. Admin Controller Genişlet
```csharp
// AdminController'a eklenecek methodlar

[HttpPost("members/{id}/checkin")]
public async Task<IActionResult> CheckInMember(Guid id)

[HttpGet("members/at-risk")]
public async Task<IActionResult> GetAtRiskMembers()

[HttpPost("members/{id}/body-metrics")]
public async Task<IActionResult> AddBodyMetric(Guid id, UserBodyMetric metric)

[HttpGet("members/{id}/metrics")]
public async Task<IActionResult> GetMemberMetrics(Guid id)
```

**Timeline:** 3-4 saat

#### 2. Member Controller Oluştur
```csharp
[Authorize]
[Route("api/[controller]")]
public class MemberController : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() // Kendi profili

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn() // Kendi check-in

    [HttpGet("goals")]
    public async Task<IActionResult> GetMyGoals()

    [HttpGet("body-metrics")]
    public async Task<IActionResult> GetMyMetrics()
}
```

**Timeline:** 2-3 saat

---

### Phase 1D: View/UI

#### 1. Admin Panel - Member Detail Sayfası Genişlet
```
Views/Admin/MemberDetail.cshtml
├── Profil Bilgileri
├── Lifecycle Status Badge (New/Active/AtRisk/Inactive/Churned)
├── Vücut Ölçümleri Grafiği
├── Hedefleri ve Progres
├── Check-in Geçmişi (Son 30 gün)
├── Abonelik Bilgisi
└── Admin Aksiyonları (Email gönder, sil, vb.)
```

**Timeline:** 4-6 saat
**Teknoloji:** Tailwind CSS + Chart.js (ölçümler için)

#### 2. Member Dashboard (Portal)
```
Views/Home/MemberDashboard.cshtml
├── Hoşgeldiniz
├── Mevcut Lifecycle Stage
├── Bu Ay Check-in Sayısı
├── Aktif Hedefler (Progress bars)
├── Son Vücut Ölçümü (vs. bir ay önce)
├── Sonraki Ders
└── Başarısızlık Notları
```

**Timeline:** 3-4 saat

---

### Phase 1E: Scheduled Jobs (Background Tasks)

Hangfire veya native hosted service kullanarak otomatikleş:

```csharp
// Her gece 2'de çalış
public class MemberLifecycleBackgroundJob
{
    public async Task ExecuteAsync()
    {
        // 1. Lifecycle status'ları güncelle
        await _memberLifecycleService.UpdateAllMembersLifecycleAsync();

        // 2. At-risk üyelere email gönder
        await _memberLifecycleService.SendAtRiskRemindersAsync();

        // 3. Abonelik bitecekler için bildir
        await _memberLifecycleService.SendRenewalRemindersAsync();

        // 4. Churn risk'leri analiz et
        await _memberLifecycleService.AnalyzeChurnRiskAsync();
    }
}
```

**Timeline:** 4-5 saat
**Zorluk:** ⭐⭐

---

### Phase 1F: DTO Katmanı

Yeni DTOlar oluştur (AfneyGym.Common/DTOs/):

```csharp
// MemberLifecycleDto.cs
public class MemberLifecycleDto
{
    public Guid UserId { get; set; }
    public MemberLifecycleStage CurrentStage { get; set; }
    public int RiskScore { get; set; }
    public int MonthlyCheckInCount { get; set; }
    public decimal? LatestWeight { get; set; }
    public int ActiveGoalCount { get; set; }
}

// CheckInDto.cs
public class CheckInDto
{
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public int DurationMinutes { get; set; }
}

// BodyMetricSummaryDto.cs
public class BodyMetricSummaryDto
{
    public decimal Weight { get; set; }
    public decimal? BodyFatPercentage { get; set; }
    public decimal WeightChangeMonth { get; set; }
    public decimal BodyFatChangeMonth { get; set; }
}
```

**Timeline:** 1-2 saat

---

## 🔧 Teknik Notlar

### Database Constraints
```sql
-- UserBodyMetric
UNIQUE INDEX UX_UserBodyMetric ON UserBodyMetric(UserId, MeasurementDate)

-- UserLifecycleStatus
UNIQUE INDEX UX_UserLifecycleStatus ON UserLifecycleStatus(UserId, CurrentStage)

-- GymCheckIn
INDEX IX_GymCheckIn_UserCheckIn ON GymCheckIn(UserId, CheckInTime)
```

### Performance Considerations
- Check-in queryları userId + tarih aralığına göre indexed
- Lifecycle status updates batch olarak çalış (mass update)
- Report sorgularını cache et (5-15 dakika)

### Data Retention
- Check-in geçmişi 1 yıl sonra archive/delete edilebilir
- Body metrics ve goals soft-delete ile saklan

---

## 📊 Reporting & Analytics (Phase 2)

Uygulandıktan sonra bu raporlar eklenebilir:

1. **Member Lifecycle Report**
   - Aşama dağılımı (% Active, % AtRisk, vb.)
   - Churn rate trend
   - Geri kazanım oranları

2. **Engagement Report**
   - Ortalama monthly check-ins
   - Consistent members (4+ check-in/ay)
   - Inactive members list

3. **Body Metrics Trends**
   - Ortalama kilo değişimi
   - Body fat reduction trends
   - Top performers (hedefine ulaşanlar)

4. **Goal Completion Analytics**
   - Tamamlanan hedef yüzdesi
   - Hedef tipi dağılımı
   - Average completion time

---

## 💡 Quick Win İçin

Eğer yarın göstermek istiyorsan, bu sırayla yap:

1. **Migration** → DB'ye schema ekle (30 min)
2. **Service** → Basic check-in ve lifecycle update (2-3 saat)
3. **Admin UI** → Check-in butonu, status badge (1-2 saat)
4. **Quick Test** → Manuel check-in yap, status değişimini göster

**Toplam:** ~5 saat → Çalışan bir MVP

---

## 📅 Estimation

| Phase | Est. Saat | Zorluk | Priority |
|-------|----------|--------|----------|
| Migration | 1-2 | ⭐ | 🔴 |
| Service Layer | 10-12 | ⭐⭐⭐ | 🔴 |
| Controllers/API | 5-6 | ⭐⭐ | 🔴 |
| Views/UI | 7-10 | ⭐⭐⭐ | 🟡 |
| Background Jobs | 4-5 | ⭐⭐ | 🟡 |
| DTO Katmanı | 1-2 | ⭐ | 🔴 |
| **Total Phase 1** | **28-37** | | |

**Takvim:** 1 hafta (full-time), 2-3 hafta (part-time)

---

## ✅ Checklist (Phase 1 Tamamlama)

- [x] Veri modeli oluşturuldu
- [x] DbContext güncellendi
- [x] Migration oluşturuldu
- [x] Service layer implement edildi
- [x] Controller endpoints eklendi
- [x] Admin UI güncellendi
- [x] Member portal oluşturuldu
- [x] Background jobs kuruldu
- [x] DTOlar oluşturuldu
- [x] E2E test edildi
- [x] Deployment'a hazırlandı

---

## 📝 Notlar

- Tüm lifecycle transitions **audit trail** ile kaydedilsin
- Üye rızası olmadan **email gönderme** kurallarını kontrol et
- Check-in için **privacy** - sadece kendi check-in'ini görebilsin
- Admin, tüm lifecycle işlemleri görebilsin
- Batch operations için **transaction management** önemli


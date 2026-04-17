# AfneyGym - Eksik Özellikler TODO Listesi

## 📊 Mevcut Durumda Olan Özellikleri
✅ **Admin Dashboard** - Temel sayaçlar ve KPI'lar  
✅ **Üye Yönetimi** - Listeleme, düzenleme, silme  
✅ **Abonelik Yönetimi** - Oluşturma, onay/ret, ödeme (Iyzico)  
✅ **Ders Yönetimi** - Oluşturma, düzenleme, kategorisiz silme  
✅ **Eğitmen Yönetimi** - CRUD işlemleri  
✅ **Yoklama Sistemi** - Ders katılım izleme  
✅ **Analytics** - Son 7 gün metrikleri, revenue raporu  
✅ **Landing Page KPI** - Aktif üye, ders, trainer sayıları  

---

## 🎯 PHASE 1: ÜYE LİFESİKLE ÖZELLİKLERİ (YÜKSEK ÖNCELİK)

### 1.1 Üye Profili Genişletmesi
- [ ] **Vücut Ölçüm Takibi** - Kilo, beden fat, kas oranı vb.
  - Veri Modeli: `UserBodyMetric` entity
  - View: Grafik ile ölçüm geçmişi
  - Priority: 🔴 HIGH | Zorluk: ⭐⭐

- [ ] **Üye Hedef Yönetimi** - Kilo hedefi, fitness hedefi vb.
  - Veri Modeli: `UserGoal` entity
  - Admin: Üyenin hedefini görebilsin
  - Priority: 🔴 HIGH | Zorluk: ⭐⭐

- [ ] **Check-in Sistemi** - Gym'e her giriş kayıt altına alınsın
  - Veri Modeli: `GymCheckIn` entity (UserId, CheckInTime, CheckOutTime)
  - API: QR kod veya manual check-in
  - Dashboard: Aylık attendance rate
  - Priority: 🔴 HIGH | Zorluk: ⭐

---

### 1.2 Üye Kategorilendirmesi
- [ ] **Risk Analizi** - Pasif/Risk altında üyeleri tanımla
  - Mantık: 30+ gün ders katılmamış, abonelik bitecek vb.
  - Admin Dashboard'da kırmızı uyarı
  - Automatic Email: "Sizi özledik, geri dön"
  - Priority: 🔴 HIGH | Zorluk: ⭐⭐

- [ ] **Üye Statüsü Otomasyonu**
  - `NewMember` → `Active` → `AtRisk` → `Inactive` → `Churned`
  - Status değişim tarihleri kaydedilsin
  - Priority: 🔴 HIGH | Zorluk: ⭐⭐⭐

---

### 1.3 Motivasyon Sistemi
- [ ] **Achievement Badges** - Ders sayısına, consistency'e göre
  - Badge tipi: `FirstLesson`, `10Lessons`, `30DaysStreak`, vb.
  - Üye profili görünsün
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

- [ ] **Leaderboard** - Ay/hafta bazında aktif üyeler
  - Ders katılım sayısına göre sırala
  - "Top 10 This Week" göster
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

---

## 💬 PHASE 2: KOMÜNİKASYON VE KAMPANYALAR (ORTA ÖNCELİK)

### 2.1 SMS/Email Kampanyaları
- [ ] **Template Sistemi** - Admin önceden hazırlı mesaj şablonları oluşturabilsin
  - ShortCode: `{UserName}`, `{DaysLeft}`, `{Gym}` vb.
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

- [ ] **Otomatik Ödeme Hatırlatması**
  - Abonelik bitişinden 7 gün öncesi
  - Abonelik bitişinden 1 gün öncesi
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

- [ ] **Ders Katılmayanlara Bildirim**
  - Kayıtlı ama gelmeyenlere 24 saat sonra
  - "Kaçtığın dersin videosunu izle" bağlantısı
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

- [ ] **Başarı Kutlaması**
  - 10. ders, 50. ders, ilk 30 gün streak vb.
  - Özelleştirilmiş başarı mesajı
  - Priority: 🟡 MEDIUM | Zorluk: ⭐

---

### 2.2 Üyelere Özel Sayfalar
- [ ] **Üye Dashboard** - Kendi verisini görebilsin
  - Mevcut üyelik bilgisi
  - Bu ay kaç derse katıldı
  - Vücut ölçüm grafiği
  - Sonraki ders nedir
  - Priority: 🔴 HIGH | Zorluk: ⭐⭐⭐

- [ ] **Üye Profil Sayfası**
  - Kendi adını, e-postasını, fotoğrafını güncellesin
  - Tercih bırakabilsin (ders tipi, saati, trainer vb.)
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

- [ ] **Ders Geçmiş Sayfası**
  - Geçmişte katıldığı tüm dersler
  - Hangi trainer, ne zaman, attendance durumu
  - Priority: 🟡 MEDIUM | Zorluk: ⭐

---

## 📈 PHASE 3: ADMIN RAPORLAMA (ORTA-DÜŞÜK ÖNCELİK)

### 3.1 Gelişmiş Raporlar
- [ ] **Üye Churn Analizi**
  - Kaç üye iptal etti, ne zaman, neden
  - Churn rate trend grafiği
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐⭐

- [ ] **Ders Popülarite Raporu**
  - En çok katılan dersler
  - En az katılan dersler
  - Trainer bazında katılım
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

- [ ] **Revenue Breakdown**
  - Ders tipi bazında revenue
  - Öğlen/Akşam farklı fiyatlandırma analytics
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐

- [ ] **Trainer Performans Raporu**
  - Her trainer'ın ders sayısı, ortalaması katılım
  - Üye memnuniyeti (ratings olursa)
  - Priority: 🟡 MEDIUM | Zorluk: ⭐⭐⭐

---

### 3.2 Export/Import
- [ ] **Excel Export**
  - Üyeleri export et
  - Dersleri export et
  - Ödeme geçmişini export et
  - Priority: 🟢 LOW | Zorluk: ⭐⭐

- [ ] **Bulk SMS/Email Gönder**
  - Filtre et (rol, durum, vb.)
  - Hepsine aynı mesajı gönder
  - Priority: 🟢 LOW | Zorluk: ⭐⭐

---

## 🔧 PHASE 4: TEKNIK İYİLEŞTİRMELER (DÜŞÜK ÖNCELİK)

### 4.1 Audit & Logging
- [ ] **Audit Log Sistemi**
  - Her admin aksiyonu kaydedilsin (üye sil, düzenle, vb.)
  - Kim, ne zaman, ne yaptı
  - Priority: 🟢 LOW | Zorluk: ⭐⭐⭐

- [ ] **Member Action History**
  - Üye ders kaydı, iptal, vb. kaydedilsin
  - Üye profil güncellemeleri
  - Priority: 🟢 LOW | Zorluk: ⭐⭐

---

### 4.2 Entegrasyonlar
- [ ] **Instagram/Facebook Paylaşım**
  - Başarısını sosyal ağlarda paylaşabilsin
  - Priority: 🟢 LOW | Zorluk: ⭐⭐

- [ ] **Google Calendar Senkronizasyonu**
  - Üye dersleri kendi calendar'ına eklesin
  - Priority: 🟢 LOW | Zorluk: ⭐⭐⭐

---

## 📋 IMPLEMENTATION PRIORITY (ÖNERİLEN SIRA)

### Quarter 1 (Bu Ay)
1. ✨ Üye Kategorilendirmesi + Risk Analizi
2. ✨ Check-in Sistemi
3. ✨ Otomatik Ödeme Hatırlatması

### Quarter 2
4. ✨ Üye Dashboard (Member Portal)
5. ✨ Vücut Ölçüm Takibi
6. ✨ Achievement Badges

### Quarter 3
7. ✨ Churn Analizi Raporu
8. ✨ Trainer Performans Raporu
9. ✨ Bulk Kampanya Gönderimi

### Quarter 4
10. ✨ Audit Log Sistemi
11. ✨ Excel Export/Import
12. ✨ Sosyal Ağ Entegrasyonları

---

## 🔑 Bileşenlerin Bir Araya Gelişi

```
CORE MEMBER LIFECYCLE:
├─ Kayıt (Registration)
├─ Onboarding (Profile setup)
├─ Active (Derslere katılım)
├─ Engagement (Check-in, badges)
├─ Risk Detection (Pasiflik analizi)
├─ Retention (Kampanyalar)
└─ Churn Prevention (Özel teklifler)

ANALYTICS:
├─ Member Metrics (Lifecycle stages)
├─ Lesson Analytics (Popularity, trainer perf)
├─ Revenue Analytics (Breakdown, forecast)
└─ Churn Prediction (Risk scores)

COMMUNICATIONS:
├─ Email Templates
├─ SMS Templates
├─ Automated Triggers
└─ Campaign Management

MEMBER EXPERIENCE:
├─ Dashboard (KPI'lar)
├─ Profile (Bilgi yönetimi)
├─ History (Geçmiş dersleri)
└─ Goals (Hedef izleme)
```

---

## 💡 Quick Wins (1-2 Gün ile Bitirilebilecek)
- ✅ Check-in butonu anasayfaya ekle
- ✅ Ödeme hatırlaması e-maili gönder
- ✅ Admin dashboard'a "Risk" üyeleri göster
- ✅ Simple status badge (New/Active/Risk/Inactive)

---

## 📝 Notlar
- Tüm yeni özellikler `IsDeleted` soft-delete pratiğini takip etmeli
- Audit trail her zaman tutulmalı
- Member lifecycle durumları enum olarak standartlaştırılmalı
- Raporlar cache edilmelidir (5-15 dakika)


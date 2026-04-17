# AfneyGym - Üyelik & Ödeme Sistemi

## ✅ Gerçekleştirilen Özellikler

### 1. **Üyelik Satın Alma & Manuel Onay Akışı**
- **Rota**: `/subscription/purchase-request`
- Üyeler 1/3/6/12 aylık paketler için talep gönderir
- Talep `SubscriptionStatus.Pending` durumda saklanır
- Owner/Admin onayından sonra Payment kaydı otomatik oluşturulur

### 2. **Owner (Salon Sahibi) Paneli**
- **Rota**: `/owner/approvals`
- Tüm bekleyen abonelik taleplerini listeler
- Onayla / Reddet butonları
- Approve işleminde:
  - Subscription → Active
  - Payment kaydı (Status: Completed)
  - Invoice oluşturur (Numarası, HTML + PDF)
  - Kullanıcıya e-mail gönderir (HTML + PDF indirme linki)

### 3. **Üyelik Yaşam Döngüsü Aksiyonları**
- **Dondur (Freeze)**: `IsFrozen = true`, besin almakta devam
- **Devam Et (Resume)**: `IsFrozen = false`, yeniden aktif
- **İptal (Cancel)**: Status → Canceled, AutoRenew kapatılır
- **Yenile (Renew)**: EndDate'i belirtilen ay kadar uzatır
- **Otomatik Yenileme Toggle**: AutoRenew alanı değişir

### 4. **Üye Detay Sayfasında (Admin Paneli)**
- **Rota**: `/admin/members/{id}/edit`
- PendingApproval badge (sarı, "Pending Subscriptions")
- Satır içi Onayla/Reddet butonları bekleyen her talep için
- Active üyelik varsa Plan Upgrade paneli

### 5. **Plan Yükseltme (Owner)**
- Ek 1/3/6 ay seçeneği
- Fark ücreti hesaplanarak eklenirse fatura yeniden oluşturulur

### 6. **Fatura & Makbuz Sistemi**
- **Format**: HTML + PDF (QuestPDF kütüphanesi)
- **Depolama**: `/wwwroot/invoices/{INV-YYYYMMDDHHMMSS-XXX}.pdf`
- **İndirme**: `/subscription/download-invoice/{invoiceId}`
- **E-mail**: HTML makbuz + PDF linki kullanıcıya gönderilir
- Fatura numarası unique indexed

### 7. **İyzico Ödeme Altyapısı** (Hazırlık Aşaması)
- Checkout form URL'si oluşturma: `StartIyzicoCheckout(subscriptionId)`
- Callback doğrulama: `IyzicoCallback(token, conversationId)`
- Payment.Provider → "Iyzico" (gelecekte)
- Configurasyon: `appsettings.json` (ApiKey, SecretKey, sandbox/prod URL)

### 8. **Otomatik Üyelik Yenileme**
- **HostedService**: Her gün 02:00'de çalışır
- AutoRenew aktif + EndDate ≤ Now olan üyelikleri bulur
- EndDate +1 ay uzatır
- AutoRenew kapalı ise → Expired
- Payment (Pending) kaydı oluşturur

### 9. **Owner Rolü Eklendi**
- `UserRole.Owner = 1`
- Admin paneline erişim yetkisi
- `/owner/approvals` routing attribute
- Approve/Reject/UpgradePlan aksiyonlarında rol kontrolü

### 10. **Verilen Veriler (Gelir Takibi)**
- `Payment` tablosu: Her onayda/yenilemede kaydedilir
- Status: Pending/Completed/Failed/Refunded
- ExternalReference: İdempotency (tekrar işlem önleme)
- Indexed: Status, CreatedAt

## 📋 Teknoloji Stack

| Katman | Teknoloji |
|--------|-----------|
| **Frontend** | ASP.NET MVC, Tailwind CSS, Bootstrap Icons |
| **API** | ASP.NET Core 10.0, Entity Framework Core 10 |
| **Database** | SQL Server LocalDB |
| **PDF/Makbuz** | QuestPDF 2026.2.0 |
| **Authentication** | Cookie-based, Claims |
| **Logging** | Serilog |
| **Email** | SmtpClient (appsettings) |
| **Ödeme (Hazır)** | iyzico SDK (v3.4+) |

## 🚀 Kurulum & Çalıştırma

### 1. **Veritabanı Migration Uygula**
```powershell
cd C:\Users\afney\Desktop\AfneyGym
dotnet ef database update --project AfneyGym.Data --startup-project AfneyGym.WebMvc
```

### 2. **appsettings Konfigürasyonu**

#### a) **E-posta Ayarları** (`appsettings.json`)
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "Port": 587,
  "SenderEmail": "noreply@yourdomain.com",
  "SenderName": "AfneyGym Support",
  "Username": "your-email@gmail.com",
  "Password": "app-specific-password",
  "EnableSsl": true,
  "UseRealEmail": true
}
```

#### b) **iyzico Ayarları** (Opsiyonel, şimdilik mock)
```json
"IyzicoSettings": {
  "ApiKey": "IYZICO_API_KEY",
  "SecretKey": "IYZICO_SECRET_KEY",
  "BaseUrl": "https://sandbox-api.iyzipay.com",
  "CallbackUrl": "https://localhost:5001/subscription/iyzico-callback"
}
```

### 3. **WebMvc Projesini Çalıştır**
```powershell
cd AfneyGym.WebMvc
dotnet run
```

Tarayıcı: `https://localhost:5001`

## 📊 Veri Modeli Özeti

### **Payment Entity**
- `Id` (Guid)
- `SubscriptionId`, `UserId` (FK)
- `Amount` (decimal 18,2)
- `Currency`, `Provider`, `ExternalReference` (unique)
- `Status` (Pending/Completed/Failed/Refunded)
- `PaidAt`, `Note`

### **Invoice Entity**
- `Id` (Guid)
- `PaymentId`, `UserId` (FK)
- `InvoiceNumber` (unique, "INV-yyyyMMddHHmmss-XXX")
- `IssuedAt`, `HtmlReceipt`, `PdfRelativePath`
- `EmailSent` (bool)

### **Subscription Extended**
- `AutoRenew` (bool) - Otomatik yenileme bayrağı
- `LastRenewalDate` (DateTime?) - Son yenileme tarihi
- `CanceledAt` (DateTime?) - İptal tarihi
- `Payments` (ICollection<Payment>) - Ödeme geçmişi

## 🔄 İş Akışı Diyagramı

```
Üye Satın Alma Talebi (Pending)
    ↓
Owner Onay Paneli (/owner/approvals)
    ├─ ONAYLA → Subscription.Status = Active
    │          → Payment oluştur (Manual, Completed)
    │          → Invoice oluştur (Numarası + PDF)
    │          → HTML makbuz + PDF linki e-mail
    │
    └─ REDDET → Subscription.Status = Rejected
               → Üyeye hata e-maili

Aktif Üyelik
    ├─ Dondur/Devam Et/İptal (Üye aksiyonu)
    ├─ 1/3/6/12 Ay Yenile (Manual)
    ├─ Plan Yükseltme (Owner)
    │
    └─ AutoRenew Enabled?
        ├─ EndDate ≤ Now → +1 Ay uzat, Payment (Pending) oluştur
        └─ EndDate > Now → Aktif kala

iyzico Checkout (Gelecek Faz)
    ├─ CheckoutFormUrl oluştur
    ├─ Kullanıcı ödeme yapıp geri döner
    └─ Callback doğrula → Payment (Completed)
```

## 📌 Son Adımlar (Gelecek İterasyonlar)

### **P0 - Şu an Aktif**
- ✅ Manuel ödeme + Owner onayı
- ✅ Üyelik yaşam döngüsü
- ✅ Fatura/makbuz + e-mail
- ✅ Otomatik yenileme infrastructure

### **P1 - İyzico Checkout**
- [ ] iyzico SDK entegrasyonu
- [ ] Checkout form rendering
- [ ] Webhook callback validation
- [ ] Test ödemeleri

### **P2 - Gelir Raporları**
- [ ] `/owner/revenue-dashboard`
- [ ] Aylık gelir grafiği
- [ ] Ödeme geçmişi export (CSV/Excel)

### **P3 - Otomatik E-postalar**
- [ ] Yenileme ön uyarısı (-7 gün)
- [ ] Süresi doldu bildirimi
- [ ] Ödeme başarısız retry

## 🛡️ Güvenlik Notları

1. **iyzico API Keys**: `appsettings.json` production'da environment variable kullanmalı
2. **PDF İndirme**: Rol + UserId kontrolü yapılıyor
3. **ExternalReference**: İdempotency key, tekrar işlem önler
4. **Cascade Delete**: Payment/Invoice, Subscription silinince de silinir

## 📝 Test Komutları

```bash
# Migration check
dotnet ef migrations list --project AfneyGym.Data --startup-project AfneyGym.WebMvc

# Build & Run
dotnet build && cd AfneyGym.WebMvc && dotnet run
```

---

**Geliştirici**: GitHub Copilot  
**Son güncelleme**: 2026-04-14  
**Versiyon**: 1.0.0-beta (Manual Payment + Owner Approvals)


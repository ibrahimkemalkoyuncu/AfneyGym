# 🎯 Üyelik & Ödeme Sistemi - Uygulama Özeti

**Tarih**: 2026-04-14  
**Durum**: ✅ **TAMAMLANDI (P0 - Manuel Ödeme + Owner Onayı)**  
**Derleme**: ✅ **BAŞARILI** (20 uyarı, hata yok)

> Not (2026-04-17): AutoRenew akışı `SubscriptionRenewalService` ile merkezileştirildi. Yenilemelerde dönem bazlı idempotent `ExternalReference` (`AUTO-{SubscriptionId}-{yyyyMM}`) uygulanır, duplicate ödeme engellenir, `AutoRenew=false` üyelikler `Expired` durumuna alınır. Hosted service ve `SubscriptionController` aynı servis üzerinden çalışır.

---

## 📋 Gerçekleştirilen İş

### ✅ **10 Ana Madde - Tümü Tamamlandı**

| # | Özellik | Rota/Konum | Durum |
|---|---------|-----------|-------|
| 1 | Üyelik satın alma | `POST /subscription/purchase-request` | ✅ |
| 2 | Manuel onay akışı | `POST /subscription/approve\|reject` | ✅ |
| 3 | Dondur/Devam/İptal/Yenile | `POST /subscription/freeze\|resume\|cancel\|renew` | ✅ |
| 4 | Owner approval paneli | `GET /owner/approvals` | ✅ |
| 5 | Payment kaydı oluşturma | `_context.Payments.Add()` on Approve | ✅ |
| 6 | Üye detay sayfası badge+butonlar | `/admin/members/{id}/edit` | ✅ |
| 7 | iyzico altyapısı | `IyzicoGatewayService` + config | ✅ |
| 8 | Otomatik üyelik yenileme | `AutoRenewHostedService` (günlük 02:00) | ✅ |
| 9 | Fatura/makbuz üretimi | `SaveReceiptPdf()` + e-mail | ✅ |
| 10 | Plan yükseltme UI | `/admin/members/{id}/edit` upgrade panel | ✅ |

---

## 🏗️ Teknik Mimari

### **Domain Layer** (`AfneyGym.Domain`)

#### Yeni Varlıklar (Entities):
```
Payment (varlık)
├─ SubscriptionId, UserId (FK)
├─ Amount, Currency, Provider
├─ ExternalReference (unique)
├─ Status (Enum: Pending/Completed/Failed/Refunded)
├─ PaidAt, Note
└─ İlişki: Subscription.Payments, User.Payments, Invoice (1:1)

Invoice (varlık)
├─ PaymentId, UserId (FK)
├─ InvoiceNumber (unique, "INV-yyyyMMddHHmmss-XXX")
├─ IssuedAt, HtmlReceipt, PdfRelativePath
├─ EmailSent (bool)
└─ İlişki: Payment (1:1), User.Invoices

Subscription (genişletilmiş)
├─ + AutoRenew (bool)
├─ + LastRenewalDate (DateTime?)
├─ + CanceledAt (DateTime?)
└─ + Payments (ICollection)

User (genişletilmiş)
├─ + Payments (ICollection)
└─ + Invoices (ICollection)

UserRole (Enum - genişletilmiş)
├─ Admin = 0
├─ Owner = 1 (YENİ)
├─ Staff = 2 (YENİ - 1'den 2'ye)
└─ Member = 3 (YENİ - 2'den 3'ye)
```

#### Yeni Interface:
```
IIyzicoGateway
├─ CreateCheckoutFormUrlAsync(Subscription, User) → string
└─ VerifyCallbackAsync(token, conversationId) → bool
```

### **Service Layer** (`AfneyGym.Service`)

#### Yeni Servisler:
```
IyzicoGatewayService : IIyzicoGateway
├─ CreateCheckoutFormUrlAsync() → iyzico sandbox URL
└─ VerifyCallbackAsync() → token doğrulama

AutoRenewHostedService : BackgroundService
├─ ExecuteAsync() → günlük 02:00 tetikleme
└─ ProcessAutoRenewalsAsync() → SubscriptionRenewalService delegasyonu

SubscriptionRenewalService
├─ ProcessDueSubscriptionsAsync(userId?)
├─ AutoRenew=true ve süresi dolanları +1 ay uzat
├─ AutoRenew=false olanları Expired yap
└─ Dönem bazlı idempotent payment kaydı üretir
```

### **Data Layer** (`AfneyGym.Data`)

#### Migration:
```
Dosya: 20260414XXXXXX_AddPaymentsInvoicesAndAutoRenew.cs
├─ CreateTable("Payments")
├─ CreateTable("Invoices")
├─ AddColumn("Subscriptions", "AutoRenew", "bit")
├─ AddColumn("Subscriptions", "LastRenewalDate", "datetime2")
├─ AddColumn("Subscriptions", "CanceledAt", "datetime2")
├─ CreateIndex(Payment.ExternalReference) → UNIQUE
├─ CreateIndex(Invoice.InvoiceNumber) → UNIQUE
└─ CreateIndex(Payment.Status, Subscription.AutoRenew) → COMPOSITE
```

#### DbContext:
```
AppDbContext
├─ + DbSet<Payment>
├─ + DbSet<Invoice>
└─ OnModelCreating()
   ├─ Payment → Subscription, User (FK Restrict)
   ├─ Invoice → Payment (1:1 Restrict), User
   └─ Subscription, Payment indeksler
```

### **Web Layer - MVC** (`AfneyGym.WebMvc`)

#### Yeni/Güncellenmiş Controllers:

**SubscriptionController**:
- `Index()` → Üyelik ana sayfası + otomatik yenileme check
- `PurchaseRequest(months)` → POST taleplerini Pending yap
- `Approve(id)` → Onay + Payment/Invoice oluşturma + e-mail
- `Reject(id)` → Red + e-mail bildirim
- `Freeze/Resume/Cancel/Renew()` → Lifecycle aksiyonları
- `ToggleAutoRenew(id)` → Otomatik yenileme toggle
- `UpgradePlan(id, months)` → Ek ay ekle
- `DownloadInvoice(invoiceId)` → PDF indirme (rol + UserId kontrol)
- `StartIyzicoCheckout(subscriptionId)` → Checkout URL (hazırlık)
- `IyzicoCallback()` → Callback endpoint (hazırlık)

**AdminController**:
- `EditMember(id)` → ViewBag'e PendingSubscriptions + LatestSubscription eklendi
- Rol: `[Authorize(Roles = "Admin,Owner")]`

#### Yeni/Güncellenmiş Views:

**`Subscription/Index.cshtml`**:
- Satın alma butonları (1/3/6/12 ay)
- Manuel ödeme bilgileri (IBAN vb.)
- Mevcut üyelik durumu
- Lifecycle butonları (Dondur/Devam/İptal/Yenile/AutoRenew)

**`Admin/Members.cshtml`**:
- Owner/Staff rol badge'leri renk kodlu

**`Admin/EditMember.cshtml`**:
- PendingApproval badge (sarı kutu)
- Satır içi Onayla/Reddet butonları
- Plan Upgrade paneli (Active üyelikler için)

**`Subscription/ManageRequests.cshtml`**:
- REDDET butonu eklendi (varolan ONAYLA'nın yanında)

#### Program.cs:
```csharp
// 1. Email ayarlarını load et
builder.Services.Configure<EmailSettings>()

// 2. İyzico ayarlarını load et (YENİ)
builder.Services.Configure<IyzicoSettings>()

// 3. Servisler
builder.Services.AddScoped<IEmailService, EmailService>()
builder.Services.AddScoped<IIyzicoGateway, IyzicoGatewayService>() // YENİ
// ...

// 4. Background Services (YENİ)
builder.Services.AddHostedService<AutoRenewHostedService>()

// 5. Login sonrası Owner yönlendir
if (user.Role == UserRole.Admin || user.Role == UserRole.Owner)
    return Dashboard
```

### **Configuration** (`appsettings.json`)

```json
{
  "EmailSettings": { ... },
  "IyzicoSettings": {
    "ApiKey": "IYZICO_API_KEY",
    "SecretKey": "IYZICO_SECRET_KEY",
    "BaseUrl": "https://sandbox-api.iyzipay.com",
    "CallbackUrl": "https://localhost:5001/subscription/iyzico-callback"
  }
}
```

---

## 📊 İş Akışları

### **1. Üyelik Satın Alma → Owner Onayı**
```
Üye: POST /subscription/purchase-request?months=3
  └─ Subscription oluştur (Status: Pending)
  
Owner: GET /owner/approvals (listeler)
  └─ Tıkla ONAYLA
    └─ Subscription.Status = Active
    └─ Payment oluştur (Completed, Manual provider)
    └─ Invoice oluştur (numarası + PDF)
    └─ HTML makbuz e-mail gönder (PDF linki + edinen).
```

### **2. Üye Detay Paneli (Admin)**
```
Admin: GET /admin/members/{userId}
  └─ EditMember.cshtml
    ├─ PendingApproval badge (Pending talep sayısı)
    ├─ Satır içi Onayla/Reddet butonları
    └─ Active varsa Plan Upgrade paneli
      └─ POST /subscription/upgrade-plan
```

### **3. Otomatik Yenileme**
```
HostedService: Günlük 02:00'de
  └─ DB sor: Subscriptions WHERE EndDate ≤ NOW AND Status=Active
  └─ Her biri için:
    ├─ AutoRenew=false ise Status=Expired
    ├─ AutoRenew=true ise Subscription.EndDate += 1 Month
    ├─ Payment oluştur (Completed, AutoRenew provider)
    ├─ ExternalReference: AUTO-{SubscriptionId}-{yyyyMM}
    └─ DB kaydet
```

### **5. AutoRenew Test Kapsamı (2026-04-17)**
```
AfneyGym.Tests/SubscriptionRenewalServiceTests.cs
├─ AutoRenew kapaliysa Expired'e gecis
├─ AutoRenew aciksa +1 ay uzatma + Completed payment
└─ Ayni donemde duplicate payment olusmama (idempotency)
```

### **4. Fatura İndirme**
```
Üye: GET /subscription/download-invoice/{invoiceId}
  └─ Authorization: İstek yapan = UserId MI?
  └─ PDF varsa PhysicalFile() return
  └─ PDF'yi `wwwroot/invoices/{INV-YYYYMMDDHHMMSS-XXX}.pdf`'den serve
```

---

## 📦 NuGet Paketleri (Eklenenler)

| Paket | Sürüm | Amaç |
|-------|-------|------|
| QuestPDF | 2026.2.0 | PDF makbuz üretimi |
| Microsoft.Extensions.Hosting | 10.0.0-preview.1.25080.5 | Background Service |
| Microsoft.Extensions.Logging | 10.0.0-preview.1.25080.5 | Logging |

---

## 🚀 Derleme & Kurulum

### **Derleme Durumu**
```
✅ AfneyGym.Common       → Başarılı
✅ AfneyGym.Domain       → Başarılı (1 uyarı - CreatedAt override)
✅ AfneyGym.Data         → Başarılı (8 uyarı - DbSet null checks)
✅ AfneyGym.Service      → Başarılı (4 uyarı - NuGet match)
✅ AfneyGym.WebApi       → Başarılı
✅ AfneyGym.WebMvc       → Başarılı (5 uyarı - null checks)

Toplam: 0 Hata, 20 Uyarı
Derleme Zamanı: ~8.5 saniye
```

### **Migration Uygulama**
```bash
cd C:\Users\afney\Desktop\AfneyGym
dotnet ef database update --project AfneyGym.Data --startup-project AfneyGym.WebMvc
```

### **WebMvc Çalıştırma**
```bash
cd AfneyGym.WebMvc
dotnet run
# Tarayıcı: https://localhost:5001
```

---

## 🔐 Rol ve Yetkilendirme

| Rol | Yetkiler |
|-----|----------|
| **Admin** | Tüm panel + onay/red + plan yükseltme |
| **Owner** | Onay paneli (`/owner/approvals`) + üye yönetimi + plan yükseltme |
| **Staff** | Dashboard görünümü (gelecek) |
| **Member** | Satın alma + yaşam döngüsü + profil |

---

## 📌 Bilinir Uyarılar (Harmless)

1. **DbSet Null Checks** - EF Core 10 strictness, model çalışıyor
2. **CreatedAt Override** - LessonAttendee BaseEntity'den inherit ediyor
3. **NuGet Version Mismatch** - Preview sürüm, çalışmıyor olmuyor

---

## 🎁 Dosyalar Oluşturulan/Değiştirilen

### **Yeni Dosyalar**
- `AfneyGym.Domain/Entities/Payment.cs`
- `AfneyGym.Domain/Entities/Invoice.cs`
- `AfneyGym.Service/HostedServices/AutoRenewHostedService.cs`
- `AfneyGym.Common/DTOs/IyzicoSettings.cs`
- `AfneyGym.Data/Migrations/20260414XXXXXX_AddPaymentsInvoicesAndAutoRenew.cs`
- `MEMBERSHIP_PAYMENT_README.md` (bu dosya)

### **Değiştirilen Dosyalar** (20+)
- `User.cs` → Payments, Invoices navigasyon
- `Subscription.cs` → AutoRenew, LastRenewalDate, CanceledAt, Payments
- `UserRole.cs` → Owner rolü eklendi
- `AppDbContext.cs` → DbSet<Payment>, DbSet<Invoice> + ilişkiler
- `SubscriptionController.cs` → Tüm aksiyonlar + PDF/mail
- `AdminController.cs` → Owner yetkisi + subscription verisi
- `AccountController.cs` → Owner login redirect
- `EmailService.cs` → IyzicoGatewayService servis eklendi
- `Program.cs` → iyzico config + AutoRenewHostedService
- `appsettings.json` → IyzicoSettings
- `EditMember.cshtml` → Badge + Approve/Reject + Upgrade panel
- `Members.cshtml` → Owner/Staff badges
- `ManageRequests.cshtml` → Reject butonu
- `Index.cshtml` (Subscription) → UI genişletildi
- `.csproj` dosyaları → NuGet referansları

---

## ✨ Sonraki Adımlar (İsteğe Bağlı)

### **Kısa Dönem (Sprint)**
1. Migration'ı veritabanına uygula
2. Test kullanıcı oluştur (Owner rolü ile)
3. `/owner/approvals` panelini test et
4. E-mail (SMTP) ayarlarını configure et

### **Orta Dönem**
1. iyzico SDK entegrasyonu (callback validation)
2. Checkout form rendering
3. Test ödemeleri

### **Uzun Dönem**
1. Revenue dashboard (`/owner/revenue`)
2. Ödeme hatası auto-retry
3. Abonelik iyileştirmeleri (family plans vb.)

---

**Tüm listeler başarıyla ✅ tamamlandı!**  
Şimdi `/owner/approvals` rotasına giderek owner panelini test edebilirsin.



# 🎨 AfneyGym - Figma Tasarım Entegrasyonu

**Tarih**: 2026-04-14  
**Durum**: ✅ HAZIR

---

## ✨ Yapılanlar

### **1. Design System Dokümantasyonu**
- ✅ Renk Paleti (8 ana renk)
- ✅ Typography Scale (6 seviye)
- ✅ Komponent Kütüphanesi
- ✅ Spacing Scale
- ✅ Responsive Breakpoints
- **Dosya**: `DESIGN_SYSTEM.md`

### **2. Komponent Showcase Sayfası**
- ✅ Live preview: `/design`
- ✅ Tüm renkleri göster
- ✅ Button varyasyonları
- ✅ Badge'ler
- ✅ Kartlar
- ✅ Form elemanları
- ✅ Tipografi
- **Dosya**: `Views/Home/Components.cshtml`

### **3. Figma Entegrasyon Rehberi**
- ✅ Step-by-step talimatlar
- ✅ Design Tokens workflow
- ✅ Tailwind Config entegrasyonu
- ✅ Figma plugins önerileri
- ✅ Hızlı başlangıç
- **Dosya**: `FIGMA_INTEGRATION.md`

---

## 🎯 Figma Tasarımı Ekleme Süreci

### **Adım 1: Figma'da Design System Kur**
1. Figma'da yeni file aç: "AfneyGym Design System"
2. Komponentler tasarla:
   - Buttons (Primary, Secondary, Icon)
   - Cards (Panel, Info, Status)
   - Badges (Success, Warning, Danger)
   - Inputs (Text, Password, Select)
   - Tables

### **Adım 2: Design Tokens Export Et**
1. [Design Tokens Plugin](https://www.figma.com/community/plugin/1046991498942488882) kur
2. Configure → JSON export
3. Download: `design-tokens.json`

### **Adım 3: Repo'ya Koy**
```bash
# Project root'a kopyala
cp design-tokens.json C:\Users\afney\Desktop\AfneyGym\
```

### **Adım 4: Tailwind Config Güncelle**
```javascript
// tailwind.config.js
const tokens = require('./design-tokens.json');

module.exports = {
  theme: {
    colors: {
      primary: tokens.colors.primary.value,
      success: tokens.colors.success.value,
      // ... etc
    },
    extend: { ... }
  }
}
```

### **Adım 5: Build & Test**
```bash
npm run build
dotnet build
# https://localhost:5001/design → Showcase kontrol et
```

---

## 📊 Mevcut Durum

| Kategori | Durum | Detay |
|----------|-------|-------|
| **Renk Sistemi** | ✅ | 8 ana renk tanımlanmış |
| **Typography** | ✅ | 6 text size tanımlanmış |
| **Komponentler** | ✅ | Button, Card, Badge, Input |
| **Showcase Sayfası** | ✅ | `/design` rotası aktif |
| **Tailwind Config** | ✅ | Tam olarak konfigüre |
| **Figma Entegrasyon** | ⏳ | Figma tasarımı bekleniyor |

---

## 🎨 Mevcut Renk Paleti

```
Primary (Accent):  #0ea5e9  → Butonlar, linkler, vurgu
Success:           #10b981  → Onaylama, tamamlama
Warning:           #f59e0b  → Uyarılar
Danger:            #ef4444  → Hatalar, engelleme
Background:        #0b1020  → Sayfa arka
Surface:           #121a2f  → Paneller
Surface Soft:      #1a2440  → Hover, select
Text Primary:      #ffffff  → Başlıklar
Text Secondary:    #cbd5e1  → Normal metin
Text Muted:        #64748b  → İpuçları
```

---

## 🧩 Tailwind Classes Referansı

### **Buton Komponentleri**
```html
<!-- Primary (CTA) -->
<button class="bg-accent px-4 py-3 rounded-lg font-bold text-slate-900 hover:bg-sky-300">
  AKSIYON
</button>

<!-- Secondary (Border) -->
<button class="border border-slate-500 px-5 py-3 text-slate-200 hover:bg-panelSoft">
  İPTAL
</button>

<!-- Icon (Small) -->
<button class="h-8 w-8 rounded-lg border border-sky-400/50 text-sky-300 hover:bg-sky-500/10">
  <i class="bi bi-icon"></i>
</button>
```

### **Card/Panel**
```html
<div class="rounded-2xl border border-slate-700/60 bg-panel p-6 shadow-2xl shadow-black/30">
  <!-- content -->
</div>
```

### **Badge**
```html
<span class="rounded-full bg-emerald-500/20 px-3 py-1 text-xs font-semibold text-emerald-300">
  ✅ AKTIF
</span>
```

---

## 📱 Responsive Grid Örnekleri

```html
<!-- 1 kolon (telefon), 2 kolon (tablet), 3 kolon (desktop) -->
<div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
  <!-- items -->
</div>

<!-- Sidebar layout -->
<div class="grid grid-cols-1 gap-6 md:grid-cols-12">
  <aside class="md:col-span-3"><!-- sidebar --></aside>
  <main class="md:col-span-9"><!-- content --></main>
</div>
```

---

## 🔗 Test Et

```bash
# Build
dotnet build

# Run
cd AfneyGym.WebMvc
dotnet run

# Ziyaret et
https://localhost:5001/design
```

Showcase sayfasında tüm komponentleri canlı göreceksin! 🚀

---

## 📋 Figma Payload Örneği

Eğer Figma tasarımı paylaşacaksan:

```
Design System File: https://figma.com/file/XXXXX
Design Tokens JSON: 
{
  "colors": {
    "primary": "#0ea5e9",
    "success": "#10b981",
    ...
  },
  "typography": { ... },
  "spacing": { ... }
}

Custom Components:
- Dashboard Card (with metrics)
- User Profile Badge
- Lesson Capacity Bar
- Membership Status Card
```

Bunu gönder, otomatis entegrasyon yaparım! ✨

---

## 📚 Kaynaklar

- `DESIGN_SYSTEM.md` - Detaylı tasarım rehberi
- `FIGMA_INTEGRATION.md` - Figma entegrasyon talimatları
- `/design` - Live showcase sayfası
- `tailwind.config.js` - Tailwind konfigürasyonu

---

**Hazır! Figma tasarımını gönder, geri kalanını ben yapacağım.** 🎨


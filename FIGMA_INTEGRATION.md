# 🎨 Figma Entegrasyonu Rehberi

**Sürüm**: 1.0  
**Tarih**: 2026-04-14  
**Durum**: Hazır

---

## 📋 Mevcut Tasarım Sistemi

AfneyGym'de tam bir **Tailwind CSS tabanlı Design System** zaten kurulmuş:

✅ **Renk Paleti** - 8 ana renk + Tailwind palette  
✅ **Typography** - 6 seviye (text-xs → text-5xl)  
✅ **Komponentler** - Button, Card, Badge, Input, Table  
✅ **Responsive** - Mobile-first (sm, md, lg, xl, 2xl)  
✅ **Icons** - Bootstrap Icons 1.11.1  

---

## 🔗 Figma ile Entegrasyon Adımları

### **1. Figma'da Design System Oluştur**

**Gerekli**:
- [ ] Color Styles (8 ana renk)
- [ ] Typography Styles (6 text size)
- [ ] Component Patterns (Button, Card, Badge)
- [ ] Spacing Tokens (p-3, p-4, p-6, gap-2, gap-4)

**Önerilen Yapı**:
```
AfneyGym Design System
├─ Colors/
│  ├─ Primary (Accent)
│  ├─ Success
│  ├─ Warning
│  ├─ Danger
│  └─ Neutrals
├─ Typography/
│  ├─ H1-H6
│  └─ Body/Caption
├─ Components/
│  ├─ Button (Primary, Secondary, Icon)
│  ├─ Card (Panel, Info, Status)
│  ├─ Badge (Variants)
│  ├─ Input (Text, Password, Select)
│  └─ Table
└─ Spacing/
   └─ Scale (4, 8, 12, 16, 24, 32px)
```

### **2. Design Tokens JSON'a Çevirme**

**Figma Plugin Kullan**: [Design Tokens](https://www.figma.com/community/plugin/1046991498942488882/Design-Tokens)

**Çıktı Örneği**:
```json
{
  "colors": {
    "primary": { "value": "#0ea5e9" },
    "success": { "value": "#10b981" },
    "danger": { "value": "#ef4444" },
    "background": { "value": "#0b1020" }
  },
  "typography": {
    "h1": { "fontSize": "30px", "fontWeight": "900" },
    "h2": { "fontSize": "24px", "fontWeight": "900" },
    "body": { "fontSize": "16px", "fontWeight": "400" }
  },
  "spacing": {
    "xs": { "value": "4px" },
    "sm": { "value": "8px" },
    "md": { "value": "12px" }
  }
}
```

Dosya: `design-tokens.json` → repo root'a kaydet

### **3. Tailwind Config'e Entegre Etme**

**tailwind.config.js** güncellemesi:
```javascript
const tokens = require('./design-tokens.json');

module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml",
  ],
  theme: {
    colors: {
      gymbg: tokens.colors.background.value,
      panel: tokens.colors.surface.value,
      accent: tokens.colors.primary.value,
      success: tokens.colors.success.value,
      danger: tokens.colors.danger.value,
      // ... extends
    },
    fontSize: {
      xs: tokens.typography.caption.fontSize,
      sm: tokens.typography.small.fontSize,
      base: tokens.typography.body.fontSize,
      // ... extends
    },
    spacing: {
      0: '0',
      1: tokens.spacing.xs.value,
      2: tokens.spacing.sm.value,
      3: tokens.spacing.md.value,
      // ... extends
    },
    extend: {
      // Custom extensions
    }
  }
}
```

### **4. CSS Classes'ı Tailwind'e Dönüştürme**

Figma'dan export edilen CSS:
```css
.button-primary {
  background-color: #0ea5e9;
  padding: 12px 16px;
  border-radius: 8px;
  font-weight: 700;
}
```

**Tailwind Equivalenti**:
```html
<button class="bg-accent px-4 py-3 rounded-lg font-bold text-slate-900">
  AKSIYON
</button>
```

### **5. Komponent Library Kurması**

**Razor PartialViews** ile reusable komponentler:

```html
<!-- Views/Shared/Components/Button.cshtml -->
@{
    var variant = ViewBag.variant ?? "primary";
    var text = ViewBag.text ?? "Button";
    var icon = ViewBag.icon;
}

<button class="@GetButtonClass(variant)">
    @if (!string.IsNullOrEmpty(icon))
    {
        <i class="bi bi-@icon me-2"></i>
    }
    @text
</button>

@functions {
    private string GetButtonClass(string variant) =>
        variant switch {
            "primary" => "bg-accent px-4 py-3 rounded-lg font-bold text-slate-900 hover:bg-sky-300",
            "secondary" => "border border-slate-500 px-5 py-3 text-slate-200 hover:bg-panelSoft",
            _ => ""
        };
}
```

### **6. Storybook/Component Showcase**

Mevcut route: **`https://localhost:5001/design`**

Bu sayfada:
- ✅ Tüm renk paletini gösterir
- ✅ Button varyasyonlarını listeler
- ✅ Badge'leri display eder
- ✅ Komponent showcase

---

## 📁 Dosya Yapısı (Önerilen)

```
project-root/
├─ design-tokens.json          # Figma export
├─ tailwind.config.js          # Tailwind config (tokens ile)
├─ DESIGN_SYSTEM.md            # Design dokümantasyonu
├─ Views/
│  ├─ Home/
│  │  └─ Components.cshtml     # Showcase sayfası
│  └─ Shared/
│     └─ Components/
│        ├─ Button.cshtml
│        ├─ Card.cshtml
│        ├─ Badge.cshtml
│        └─ Input.cshtml
└─ wwwroot/
   └─ css/
      └─ tailwind.css          # Generated
```

---

## 🔄 Figma → Kod Workflow

### **Seçenek 1: Manuel (Basit)**
```
Figma Design
    ↓ (Export JSON)
design-tokens.json
    ↓ (Manual review)
tailwind.config.js güncellemesi
    ↓
npm run build
    ↓
wwwroot/css/tailwind.css
```

### **Seçenek 2: Otomatis (Gelişmiş)**
```
Figma Design
    ↓ (GitHub Action Trigger)
Design Tokens Export API
    ↓ (Auto-commit)
design-tokens.json → GitHub
    ↓ (Webhook)
Azure/CI/CD Pipeline
    ↓
tailwind.config.js generate
    ↓
Deploy
```

### **Seçenek 3: Live (Premium)**
```
Figma Inspect Mode
    ↓ (Copy Tailwind CSS)
Direct to HTML/View
    ↓
Real-time preview
```

---

## 📝 Figma Plugins Önerileri

| Plugin | Amaç | Link |
|--------|------|------|
| **Design Tokens** | Token export | [✓ Community](https://www.figma.com/community/plugin/1046991498942488882) |
| **Tailwind CSS** | Tailwind to Figma | [✓ Community](https://www.figma.com/community/plugin/1159422159292519498) |
| **Storybook** | Komponent docs | [✓ Official](https://www.figma.com/community/plugin/1088899657675472815) |
| **CSS Gen** | CSS output | [✓ Community](https://www.figma.com/community/plugin/746318899744347786) |

---

## ✅ Checklist - Figma Entegrasyonu

- [ ] Figma projesi oluştur
- [ ] Design System componentleri tasarla
- [ ] Design Tokens JSON export et
- [ ] `design-tokens.json` → repo root
- [ ] `tailwind.config.js` güncellemesi
- [ ] Test build: `npm run build` / `dotnet build`
- [ ] `/design` showcase sayfasında kontrol et
- [ ] Razor PartialViews komponentleri oluştur
- [ ] Dokümantasyon güncelle
- [ ] GitHub Actions workflow kur (opsiyonel)

---

## 🚀 Hızlı Başlangıç (Mevcut Durum)

Figma'sız olarak başlamak için:

1. Mevcut tasarımı kullan (Tailwind CSS)
2. `/design` sayfasında showcase gör
3. Komponentleri kopyala/yapıştır
4. Tailwind class'larını customize et

**Figma hazırlanırken**:
- Design Tokens çıkart
- `design-tokens.json` gönder
- Otomatis entegrasyon yapacağım

---

## 📧 Figma'yı Paylaşmak İçin

Eğer Figma tasarımı varsa:

1. **Figma URL'i gönder** (View-only access)
2. **Design Tokens JSON'ı paylaş**
3. **Custom komponentler listesini yaz**

Örnek:
```
Figma: https://www.figma.com/file/xxxxx
Tokens: design-tokens.json (attached)
Custom Components: 
  - Dashboard Card (with metrics)
  - User Avatar Badge
  - Lesson Card (with capacity bar)
```

Sonra otomatik entegrasyon yaparım! ✨

---

## 📚 Kaynaklar

- [Tailwind CSS Docs](https://tailwindcss.com)
- [Design Tokens Format](https://design-tokens.github.io/format/)
- [Figma Design Tokens Plugin](https://www.figma.com/community/plugin/1046991498942488882)
- [Storybook Integration](https://storybook.js.org/)

---

**Durum**: Design System hazır, Figma entegrasyonu için bekliyor. 🎨


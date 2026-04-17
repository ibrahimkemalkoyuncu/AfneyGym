# 🎨 AfneyGym Design System

**Versiyon**: 1.0.0  
**Tarih**: 2026-04-14  
**Framework**: Tailwind CSS + Bootstrap Icons  
**Tema**: Dark Mode (Gym/Modern)

---

## 📊 Mevcut Renk Paleti

### **Tailwind Config** (`tailwind.config.js`)
```javascript
theme: {
    extend: {
        colors: {
            // Temel Renkler
            gymbg: "#0b1020",      // Arka plan (çok koyu)
            panel: "#121a2f",      // Panel arka plan
            panelSoft: "#1a2440",  // Panel soft arka (hover)
            accent: "#0ea5e9"      // Ana renk (Sky Blue)
        }
    }
}
```

### **Renk Kullanımı**

| Renk | Hex | Kullanım | Örnek |
|------|-----|----------|-------|
| **Primary/Accent** | `#0ea5e9` | CTA butonları, active states | Giriş Yap, Onayla |
| **Success** | `#10b981` (emerald-500) | Onaylama, tamamlama | ✅ GİTTİ, Aktif |
| **Warning** | `#f59e0b` (amber-500) | Uyarı, az kaldı | ⚠️ AZ KALDI |
| **Danger** | `#ef4444` (red-500) | Hata, engelleme | ❌ DOLDU, RED |
| **Info** | `#3b82f6` (blue-500) | Bilgi, detaylar | 📧 Onayla/Red |
| **Background** | `#0b1020` | Sayfa arka | body bg |
| **Surface** | `#121a2f` | Panel, card | Panel bg |
| **Surface Soft** | `#1a2440` | Hover, select | Hover state |
| **Text Primary** | `#ffffff` (white) | Başlık, vurgu | H2, Bold |
| **Text Secondary** | `#cbd5e1` (slate-300) | Normal metin | Paragraf |
| **Text Muted** | `#64748b` (slate-500) | İpucu, yardımcı | Small, helper |

---

## 🧩 Komponent Kütüphanesi

### **1. Button Varyasyonları**

#### Primary Button (CTA)
```html
<button class="inline-flex items-center justify-center gap-2 rounded-lg bg-accent px-4 py-3 text-sm font-bold text-slate-900 transition hover:bg-sky-300">
    <i class="bi bi-icon"></i> AKSIYON
</button>
```
- **Kullanım**: Giriş Yap, Onayla, Kaydet
- **Hover**: `hover:bg-sky-300` (açılır)
- **Padding**: `px-4 py-3` (medium)

#### Secondary Button (Border)
```html
<button class="inline-flex items-center justify-center gap-2 rounded-lg border border-slate-500 px-5 py-3 text-sm font-semibold text-slate-200 hover:bg-panelSoft">
    İPTAL
</button>
```
- **Kullanım**: İptal, Geri
- **Border**: `border-slate-500`
- **Hover**: `hover:bg-panelSoft`

#### Icon Button (Small)
```html
<button class="inline-flex h-8 w-8 items-center justify-center rounded-lg border border-sky-400/50 text-sky-300 hover:bg-sky-500/10">
    <i class="bi bi-pencil-square"></i>
</button>
```
- **Kullanım**: Edit, Delete, View
- **Boyut**: `h-8 w-8` (24x24px)
- **Renk**: Contexual (`sky-300`, `rose-300`, `emerald-300`)

### **2. Card/Panel Varyasyonları**

#### Panel Card
```html
<div class="rounded-2xl border border-slate-700/60 bg-panel p-6 shadow-2xl shadow-black/30">
    <h3 class="text-lg font-black text-white">Başlık</h3>
    <p class="mt-2 text-sm text-slate-400">İçerik...</p>
</div>
```
- **Border**: `border-slate-700/60` (60% opacity)
- **Shadow**: `shadow-2xl shadow-black/30`
- **Padding**: `p-6` (standard)

#### Info Card (Compact)
```html
<div class="rounded-xl border border-slate-700/60 bg-panel p-4">
    <div class="text-xs uppercase tracking-wide text-slate-400 mb-1">BAŞLIK</div>
    <div class="text-3xl font-black text-white">99</div>
</div>
```
- **Padding**: `p-4` (compact)
- **Typography**: `text-xs uppercase` başlık

### **3. Input Varyasyonları**

#### Text Input
```html
<div class="relative">
    <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3 text-slate-500">
        <i class="bi bi-icon"></i>
    </div>
    <input class="block w-full rounded-lg border border-slate-600 bg-panelSoft py-3 pl-10 pr-3 text-sm text-white placeholder-slate-500 focus:border-accent focus:outline-none focus:ring-2 focus:ring-accent/30" placeholder="Gir..." />
</div>
```
- **Border**: `border-slate-600`
- **Background**: `bg-panelSoft`
- **Focus**: `focus:border-accent focus:ring-accent/30`
- **Icon**: Sol taraf (`pl-10`)

#### Password Input with Toggle
```html
<div class="relative">
    <input type="password" class="..." />
    <button type="button" class="absolute inset-y-0 right-0 flex items-center pr-3 text-slate-500 hover:text-accent transition">
        <i class="bi bi-eye"></i>
    </button>
</div>
```

### **4. Badge Varyasyonları**

#### Status Badge (Success)
```html
<span class="inline-flex rounded-full bg-emerald-500/20 px-3 py-1 text-xs font-semibold text-emerald-300">
    ✅ AKTIF
</span>
```

#### Status Badge (Danger)
```html
<span class="inline-flex rounded-full bg-rose-500/20 px-2 py-1 text-xs font-semibold text-rose-300">
    ❌ DOLDU
</span>
```

#### Status Badge (Warning)
```html
<span class="inline-flex rounded-full bg-amber-500/20 px-2 py-1 text-xs font-bold text-amber-300">
    ⚠️ AZ KALDI
</span>
```

- **Pattern**: `bg-{color}-500/20` + `text-{color}-300`
- **Padding**: `px-2 py-1` to `px-3 py-1`

### **5. Table Varyasyonu**

```html
<div class="overflow-hidden rounded-2xl border border-slate-700/60 bg-panel shadow-2xl">
    <table class="min-w-full divide-y divide-slate-700/60 text-sm">
        <thead class="bg-panelSoft">
            <tr class="text-left text-xs uppercase tracking-wide text-slate-400">
                <th class="px-5 py-3">BAŞLIK</th>
            </tr>
        </thead>
        <tbody class="divide-y divide-slate-800/70">
            <tr class="hover:bg-slate-800/40">
                <td class="px-5 py-4">...</td>
            </tr>
        </tbody>
    </table>
</div>
```
- **Header**: `bg-panelSoft`
- **Rows**: `hover:bg-slate-800/40`
- **Dividers**: `divide-slate-800/70`

### **6. Form Validation**

#### Error Summary
```html
<div asp-validation-summary="ModelOnly" class="rounded-lg border border-rose-400/30 bg-rose-500/10 p-3 text-sm text-rose-200"></div>
```

#### Field Error
```html
<span asp-validation-for="Field" class="mt-1 block text-xs text-rose-300"></span>
```

---

## 📐 Spacing Scale

| Token | Değer | Kullanım |
|-------|-------|----------|
| `p-3` | 12px | Compact |
| `p-4` | 16px | Small |
| `p-5` | 20px | Medium |
| `p-6` | 24px | Large |
| `p-8` | 32px | XL |
| `gap-2` | 8px | Tight |
| `gap-3` | 12px | Medium |
| `gap-4` | 16px | Large |
| `mb-3` | 12px | Margin bottom |
| `mt-2` | 8px | Margin top |

---

## 🔤 Typography

### **Tailwind Scale**
```
text-xs   → 12px  (Helper, Badge)
text-sm   → 14px  (Body, Form)
text-base → 16px  (Default)
text-lg   → 18px  (Card Title)
text-xl   → 20px  (Section Title)
text-2xl  → 24px  (Page Title)
text-3xl  → 30px  (Hero Title)
```

### **Font Weight**
```
font-normal     → 400  (Body text)
font-semibold   → 600  (Button, Label)
font-bold       → 700  (Emphasis)
font-black      → 900  (Heading, Hero)
```

### **Text Colors**
```
text-white        → Başlık
text-slate-200    → Button Text
text-slate-300    → Body Text
text-slate-400    → Secondary Text
text-slate-500    → Muted Text
text-accent       → Link, CTA
```

---

## 🎯 Komponent Layout

### **Sidebar Navigation**
```css
.sidebar {
    width: 260px;
    height: 100vh;
    position: fixed;
    background: #121a2f;
    border-right: 1px solid rgba(148, 163, 184, 0.25);
}

.nav-link {
    color: #cbd5e1;
    padding: 12px 25px;
    display: flex;
    align-items: center;
    transition: 0.3s;
    border-left: 3px solid transparent;
}

.nav-link:hover, .nav-link.active {
    color: #fff;
    background: #1a2440;
    border-left-color: #0ea5e9;
}
```

### **Main Content**
```css
.main-content {
    margin-left: 260px;
    padding: 30px;
    max-width: 1200px;
}
```

---

## 🎨 Figma Entegrasyonu

### **Figma → Tailwind Akışı**

1. **Figma'da Desing System** oluştur:
   - Color Styles
   - Typography Styles
   - Component Patterns

2. **Figma Plugins** kullan:
   - Tailwind CSS (Export)
   - Storybook Integration
   - Design Token Export

3. **CSS Modules** çıkar:
   ```json
   {
     "colors": {
       "primary": "#0ea5e9",
       "success": "#10b981"
     },
     "spacing": {
       "xs": "4px",
       "sm": "8px",
       "md": "12px"
     }
   }
   ```

4. **Tailwind Config** güncelle:
   ```javascript
   const tokens = require('./design-tokens.json');
   
   module.exports = {
     theme: {
       colors: tokens.colors,
       spacing: tokens.spacing,
       extend: { ... }
     }
   }
   ```

---

## 📱 Responsive Breakpoints

```css
sm   640px    /* Telefon */
md   768px    /* Tablet */
lg   1024px   /* Desktop */
xl   1280px   /* Wide */
2xl  1536px   /* Ultra Wide */
```

### **Örnek Kullanım**
```html
<div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
    <!-- 1 kolon (telefon), 2 kolon (tablet), 3 kolon (desktop) -->
</div>
```

---

## 🎯 Komponent Showcase Sayfası

Şu anda yapılabilir bir **Storybook** ya da **Components** showcase sayfası:

```
/components
├─ buttons
│  ├─ primary.html
│  ├─ secondary.html
│  └─ icon.html
├─ cards
│  ├─ panel.html
│  ├─ info-card.html
│  └─ status-card.html
├─ forms
│  ├─ text-input.html
│  ├─ password-input.html
│  └─ select.html
└─ tables
   └─ data-table.html
```

---

## 📝 Figma Dosya Paylaşımı

Eğer Figma tasarımın varsa:
1. **Figma URL'i paylaş** (View-only link)
2. **Design Tokens** JSON formatında gönder
3. **Component Library** Figma komponentlerini belirt

Ben bunları Tailwind CSS'e otomatik çevirebilirim.

---

## ✅ Şu Anda Uygulanan

- ✅ Tailwind CSS (Complete)
- ✅ Dark Theme (Consistent)
- ✅ Color Palette (Defined)
- ✅ Typography Scale (Set)
- ✅ Component Patterns (Established)
- ✅ Responsive Design (Mobile-first)
- ✅ Bootstrap Icons (Integrated)

---

**Figma Tasarımı Eklemek İçin**:
1. Figma projesini paylaş
2. Design Tokens'ları gönder
3. Custom bileşenler listesini yaz

Oto-konversiyon yaparım! 🎨


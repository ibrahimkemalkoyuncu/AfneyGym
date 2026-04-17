# AfneyGym Kullanıcı El Kitabı (Admin)

## 1. Eğitmen Yönetimi (CRUD Senaryosu)

Sistemdeki eğitmen kadrosunu yönetmek için Dashboard üzerindeki **"Eğitmenler"** sekmesini kullanın.

### A. Yeni Eğitmen Ekleme (Create)
1. Yan menüden **Eğitmenler**'e tıklayın.
2. Sağ üstteki **+ YENİ EĞİTMEN** butonuna basın.
3. **Senaryo:** "Caner Demir" isimli, "Fitness" branşında bir eğitmen ekleyelim.
4. Fotoğraf alanına eğitmenin vesikalık veya profesyonel fotoğrafını yükleyin.
5. **Kaydet** butonuna bastığınızda fotoğraf sunucuda şifrelenir ve liste ekranına dönersiniz.

### B. Eğitmen Listeleme (Read)
1. Kaydettiğiniz tüm eğitmenler modern kart tasarımıyla listelenir.
2. Resimlerin üzerinde branş etiketlerini (Fitness, Yoga vb.) görebilirsiniz.
3. Kart üzerindeki metinler 2 satırla sınırlıdır; detaylar düzenleme ekranındadır.

### C. Eğitmen Bilgilerini Güncelleme (Update)
1. **Senaryo:** Caner Demir'in branşını "BodyBuilding" olarak değiştirmek istiyoruz.
2. Kartın altındaki **DÜZENLE** butonuna basın.
3. Formda mevcut bilgiler ve mevcut fotoğraf yüklü gelecektir.
4. Bilgiyi güncelleyin. Eğer fotoğrafı değiştirmek istemiyorsanız dosya alanını boş bırakın.
5. **Değişiklikleri Kaydet** dediğinizde eski veri güncellenir.

### D. Eğitmen Silme (Delete)
1. Silmek istediğiniz eğitmen kartının altındaki **SİL** butonuna basın.
2. Tarayıcı size "Emin misiniz?" uyarısı çıkaracaktır.
3. Onayladığınızda; eğitmen veritabanından silinir ve sunucudaki fotoğraf dosyası depolama alanından fiziksel olarak kaldırılır.

## 2. Kullanıcı Yönetimi
- Tüm kayıtlı kullanıcılar (Yönetici, Personel, Üye) bu listede görünür.
- Sağ üstteki arama çubuğu ile isim veya e-posta üzerinden anlık filtreleme yapabilirsiniz.

## 3. Ders Yönetimi
- Haftalık ders programını oluşturmak için **Dersler** sekmesini kullanın.
- Ders eklerken eğitmen ve şube seçimi zorunludur.

## 4. Git Akışı Eğitimi

Bu bölüm, ekip içinde güvenli ve düzenli geliştirme için kullanılan temel Git adımlarını açıklar.

### A. Yeni Feature Branch Açma

**Nedir?**
Feature branch, yeni bir özellik veya düzeltme için ana daldan (genelde `main` veya `develop`) ayrılarak oluşturulan geçici çalışma dalıdır.

**Ne işe yarar?**
1. Ana dalın stabil kalmasını sağlar.
2. Farklı geliştirmeleri birbirinden izole eder.
3. Kod inceleme sürecini daha anlaşılır hale getirir.

**Örnek komutlar:**
```bash
git checkout main
git pull
git checkout -b feature/kullanici-profil-guncelleme
```

### B. Değişiklikleri Commit/Push

**Nedir?**
1. `commit`: Yapılan değişiklikleri yerel depoda açıklama mesajı ile kaydetmektir.
2. `push`: Yereldeki commit'leri uzak depoya (GitHub) göndermektir.

**Ne işe yarar?**
1. Yapılan işin geçmişi takip edilebilir olur.
2. Ekip arkadaşları değişiklikleri görebilir.
3. CI kontrolleri ve PR süreçleri tetiklenir.

**Örnek komutlar:**
```bash
git add .
git commit -m "Profil sayfasina guncelleme eklendi"
git push -u origin feature/kullanici-profil-guncelleme
```

### C. Otomatik PR Oluşturma

**Nedir?**
Pull Request (PR), feature branch'teki değişiklikleri hedef dala birleştirme talebidir. Otomatik PR oluşturma ise bu talebin komut satırıyla hızlıca açılmasıdır.

**Ne işe yarar?**
1. Kod inceleme ve kalite kontrol sürecini başlatır.
2. Branch protection kurallarına uygun merge akışı sağlar.
3. Build/test sonuçlarına göre güvenli birleştirme yapılmasına yardımcı olur.

**Örnek komut (GitHub CLI):**
```bash
gh pr create --base main --head feature/kullanici-profil-guncelleme --title "Profil guncelleme ozelligi" --body "Yapilan degisikliklerin ozeti"
```
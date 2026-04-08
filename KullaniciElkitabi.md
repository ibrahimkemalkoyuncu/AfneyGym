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
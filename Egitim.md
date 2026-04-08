# GitHub Akisi Egitim Notu

## 1. Yeni feature branch acma

### Nedir?
Feature branch, yeni bir ozelligi veya duzeltmeyi ana gelistirme dalindan (genelde `main` veya `develop`) ayri bir dalda gelistirmek icin olusturulan gecici calisma dalidir.

### Ne ise yarar?
- Ana dali stabil tutar.
- Birden fazla isi birbirinden ayirir.
- Hata oldugunda yalnizca ilgili branch'i etkiler.
- Kod inceleme (PR) surecini daha temiz hale getirir.

### Kisa ornek
```bash
git checkout main
git pull
git checkout -b feature/kullanici-profil-guncelleme
```

## 2. Degisiklikleri commit/push

### Nedir?
- `commit`: Yaptigin degisikliklerin yerel depoda aciklama metniyle kayit altina alinmasidir.
- `push`: Yereldeki commit'lerin GitHub'daki uzak depoya gonderilmesidir.

### Ne ise yarar?
- Yapilan isin gecmisi izlenebilir olur.
- Ekip uyeleri degisiklikleri gorebilir.
- CI/CD ve PR gibi otomasyonlar tetiklenir.

### Kisa ornek
```bash
git add .
git commit -m "Profil sayfasina guncelleme eklendi"
git push -u origin feature/kullanici-profil-guncelleme
```

## 3. Otomatik PR olusturma

### Nedir?
PR (Pull Request), feature branch'teki degisiklikleri hedef dala (or. `main`) birlestirme talebidir. Otomatik PR olusturma ise bu talebin komut satiri veya araclarla otomatik acilmasidir.

### Ne ise yarar?
- Kod inceleme ve onay surecini baslatir.
- Branch protection kurallariyla kalite guvencesi saglar.
- Build/test sonucuna gore guvenli merge imkani verir.

### Kisa ornek (GitHub CLI)
```bash
gh pr create --base main --head feature/kullanici-profil-guncelleme --title "Profil guncelleme ozelligi" --body "Yapilan degisikliklerin ozeti"
```

## Onerilen Akis Ozeti

1. Feature branch ac.
2. Degisiklik yap.
3. Commit ve push yap.
4. PR olustur.
5. Build/test ve inceleme tamamlaninca merge et.

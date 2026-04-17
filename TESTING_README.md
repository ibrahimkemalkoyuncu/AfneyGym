# AfneyGym Test Rehberi

Bu iterasyonda iki test projesi eklendi:

- `AfneyGym.Tests` -> servis davranisi unit testleri (`JoinLessonStatus`, `CancelJoinStatus`)
- `AfneyGym.E2E` -> Playwright tabanli gercek tarayici smoke senaryolari

## Hızlı Çalıştırma

```powershell
dotnet test "C:\Users\afney\Desktop\AfneyGym\AfneyGym.Tests\AfneyGym.Tests.csproj"
```

## E2E Çalıştırma

1) Uygulamayı ayrı terminalde ayağa kaldırın.
2) İlk kurulumda Playwright browser paketlerini yükleyin.

```powershell
$env:AFNEYGYM_RUN_E2E="1"
$env:AFNEYGYM_BASE_URL="http://localhost:5171"
dotnet build "C:\Users\afney\Desktop\AfneyGym\AfneyGym.E2E\AfneyGym.E2E.csproj"
pwsh "C:\Users\afney\Desktop\AfneyGym\AfneyGym.E2E\bin\Debug\net10.0\playwright.ps1" install

dotnet test "C:\Users\afney\Desktop\AfneyGym\AfneyGym.E2E\AfneyGym.E2E.csproj"
```

`AFNEYGYM_RUN_E2E` degiskeni verilmezse E2E testleri kendini skip eder.

## E2E Smoke Kapsami

- Login sayfasinda sifre goste/gizle
- Ana sayfa yuklenmesi
- Hatali giris mesaji
- Classes sayfasi yuklenmesi
- Anonim kullanici icin Member dashboard login redirect


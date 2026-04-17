# AfneyGym Deployment Readiness

Bu dokuman, `AfneyGym.WebMvc` uygulamasini staging/production ortamina cikarmadan once minimum kontrolleri listeler.

## 1) Build ve Test

```powershell
dotnet build "C:\Users\afney\Desktop\AfneyGym\AfneyGym.slnx"
dotnet test "C:\Users\afney\Desktop\AfneyGym\AfneyGym.Tests\AfneyGym.Tests.csproj"
```

## 2) Migration

```powershell
dotnet ef database update --project "C:\Users\afney\Desktop\AfneyGym\AfneyGym.Data\AfneyGym.Data.csproj" --startup-project "C:\Users\afney\Desktop\AfneyGym\AfneyGym.WebMvc\AfneyGym.WebMvc.csproj" --context AppDbContext
```

## 3) Runtime Health Endpoints

Uygulama ayaga kalktiktan sonra:

- `GET /health`
- `GET /ready`

Ornek:

```powershell
Invoke-WebRequest "http://localhost:5171/health" -UseBasicParsing
Invoke-WebRequest "http://localhost:5171/ready" -UseBasicParsing
```

## 4) Minimum Smoke Kontrolleri

- Ana sayfa aciliyor (`/`)
- Login sayfasi aciliyor (`/Account/Login`)
- Classes sayfasi aciliyor (`/classes`)
- Admin login sonrasi dashboard aciliyor (`/Admin/Dashboard`)
- Member login sonrasi dashboard aciliyor (`/Member/Dashboard`)

## 5) Hosted Services Notu

Asagidaki servisler startup'ta calisir:

- `AutoRenewHostedService`
- `LessonReminderHostedService`
- `MemberLifecycleHostedService`

Deployment oncesi SMTP/odeme ayarlari dogru degilse test ortaminda yan etkileri onlemek icin ilgili konfig kontrol edilmelidir.

## 6) Log ve Geri Alma

- Serilog dosya cikisi: `AfneyGym.WebMvc/Logs/`
- Migration geri alma (gerekiyorsa):

```powershell
dotnet ef database update <PreviousMigrationName> --project "C:\Users\afney\Desktop\AfneyGym\AfneyGym.Data\AfneyGym.Data.csproj" --startup-project "C:\Users\afney\Desktop\AfneyGym\AfneyGym.WebMvc\AfneyGym.WebMvc.csproj" --context AppDbContext
```


# AfneyGym Projeyi Ayaga Kaldirma

## 1) Klasore gir
```powershell
cd C:\Users\afney\Desktop\AfneyGym
```

## 2) Paketleri yukle ve derle
```powershell
dotnet restore .\AfneyGym.slnx
dotnet build .\AfneyGym.slnx
```

## 3) Veritabani migration uygula
```powershell
dotnet ef database update --project .\AfneyGym.Data\AfneyGym.Data.csproj --startup-project .\AfneyGym.WebMvc\AfneyGym.WebMvc.csproj --context AppDbContext
```

## 4) Web MVC uygulamasini calistir
```powershell
dotnet run --project .\AfneyGym.WebMvc\AfneyGym.WebMvc.csproj
```

## 5) (Opsiyonel) Web API calistir
Ayrica API'yi acmak istersen ayri terminalde:
```powershell
dotnet run --project .\AfneyGym.WebApi\AfneyGym.WebApi.csproj
```

## Not
- WebMvc tarafi genelde `https://localhost:xxxx` veya `http://localhost:xxxx` adresinde acilir.
- Port bilgisini terminal cikti satirindaki `Now listening on:` kismindan gorebilirsin.

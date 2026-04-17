# AfneyGym

AfneyGym is a layered .NET solution for gym management. The repository contains a Web MVC app, a Web API, domain and service layers, and EF Core data access.

## Solution Structure

- AfneyGym.Common: Shared DTOs and cross-cutting primitives
- AfneyGym.Domain: Entities and service interfaces
- AfneyGym.Data: EF Core DbContext and migrations
- AfneyGym.Service: Business services
- AfneyGym.WebApi: API endpoints
- AfneyGym.WebMvc: MVC web application

## Tech Stack

- .NET 10 (preview)
- ASP.NET Core MVC and Web API
- Entity Framework Core (SQL Server)
- Serilog (WebMvc)

## Getting Started

### Prerequisites

- .NET SDK 10 preview
- SQL Server (or SQL Server Express)

### Restore and Build

```bash
dotnet restore AfneyGym.slnx
dotnet build AfneyGym.slnx
```

### Run Web API

```bash
dotnet run --project AfneyGym.WebApi/AfneyGym.WebApi.csproj
```

### Run MVC App

```bash
dotnet run --project AfneyGym.WebMvc/AfneyGym.WebMvc.csproj
```

## Test and Deployment Docs

- Test guide: `TESTING_README.md`
- Deployment readiness: `DEPLOYMENT_README.md`

> Not: Depoda su an zorunlu bir `.github/workflows` pipeline dosyasi bulunmuyor.

## Branching Suggestion

- main: Production-ready branch
- develop: Integration branch
- feature/*: Feature branches merged via pull request

## PR Validation Checklist

Use this quick flow once to verify branch protection is enforced as expected.

1. Create a branch from develop:

```bash
git checkout develop
git checkout -b feature/pr-protection-test
```

2. Make a small change, commit, and push:

```bash
git add README.md
git commit -m "Test PR protection flow"
git push -u origin feature/pr-protection-test
```

3. Open a pull request to main and verify:

- Direct merge is blocked until checks finish.
- CI check `build` is required.
- At least 1 approval is required.
- Conversation resolution is required.

4. Merge after checks and approval, then clean branch:

```bash
git checkout develop
git branch -D feature/pr-protection-test
git push origin --delete feature/pr-protection-test
```

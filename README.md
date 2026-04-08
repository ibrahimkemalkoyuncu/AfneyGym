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

## CI

GitHub Actions workflow is defined in .github/workflows/dotnet-ci.yml and runs restore and build on pushes and pull requests.

## Branching Suggestion

- main: Production-ready branch
- develop: Integration branch
- feature/*: Feature branches merged via pull request

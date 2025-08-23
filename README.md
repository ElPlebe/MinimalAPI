# Minimal APIs in .NET – Demo Project

This repository contains a complete, ready-to-run **.NET 8 Minimal API** with:
- Feature-based structure
- Route Groups for versioning (`/api/v1`)
- Endpoint Filters for simple validation
- ProblemDetails for consistent error responses
- Swagger/OpenAPI for interactive docs
- xUnit tests using `WebApplicationFactory<Program>`

## Prerequisites
- .NET SDK 8.x

## Run the API
```bash
dotnet restore
dotnet run --project src/Api/Api.csproj
# Browse Swagger UI at http://localhost:5000/swagger (or the port shown)
```

## Try requests (REST Client/VS Code)
Open `requests/api.http` and send requests, or use curl:
```bash
curl http://localhost:5000/api/v1/todos
```

## Run tests
```bash
dotnet test tests/Api.Tests/Api.Tests.csproj
```

## Project layout
```
/src/Api
  Program.cs
  Api.csproj
  /Features/Todos
    TodoContracts.cs
    TodoEndpoints.cs
    TodoRepository.cs
    TitleNotEmptyFilter.cs
/tests/Api.Tests
  Api.Tests.csproj
  MinimalApiTests.cs
/requests
  api.http
Directory.Build.props
```

## Notes
- Storage is **in-memory** via `InMemoryTodoRepository` — **no database required**.
- Swap the repository with EF Core later if needed.

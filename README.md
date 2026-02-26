# AssuranceService (.NET 8 Minimal API + Clean Architecture)

## Structure
- Api : Minimal API (Swagger, endpoints)
- Application : CQRS (MediatR), validation (FluentValidation), interfaces
- Domain : Entités métier pures (Policy, Customer)
- Infrastructure : EF Core (SqlServer), repositories, DbContext

## Démarrage local
```bash
dotnet restore ./src/Api/AssuranceService.Api.csproj
dotnet run --project ./src/Api/AssuranceService.Api.csproj
```
Swagger: http://localhost:5243/swagger (port réel selon dotnet)

## Docker
```bash
docker build -t assuranceservice:latest -f src/Api/Dockerfile .
docker run -p 8080:8080 --name assuranceservice assuranceservice:latest
```

## Endpoints
- POST /policies
- GET /policies/{id}

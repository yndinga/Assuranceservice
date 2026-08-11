# syntax=docker/dockerfile:1
# Contexte de build : racine microservice (D:\dev_netcore\microservice)
#   docker build -f AssuranceService/Dockerfile -t ... .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Contrats référentiel (projet partagé)
COPY ["ReferentielService/src/Contracts/ReferentielService.Contracts.csproj", "ReferentielService/src/Contracts/"]
COPY ["ReferentielService/src/Contracts/Events.cs", "ReferentielService/src/Contracts/"]
COPY ["ReferentielService/src/Contracts/Events/", "ReferentielService/src/Contracts/Events/"]

COPY ["AssuranceService/AssuranceService.sln", "AssuranceService/"]
COPY ["AssuranceService/src/Api/AssuranceService.Api.csproj", "AssuranceService/src/Api/"]
COPY ["AssuranceService/src/Application/AssuranceService.Application.csproj", "AssuranceService/src/Application/"]
COPY ["AssuranceService/src/Domain/AssuranceService.Domain.csproj", "AssuranceService/src/Domain/"]
COPY ["AssuranceService/src/Infrastructure/AssuranceService.Infrastructure.csproj", "AssuranceService/src/Infrastructure/"]

WORKDIR /src/AssuranceService
RUN dotnet restore "src/Api/AssuranceService.Api.csproj"

COPY AssuranceService/src/ src/
RUN dotnet publish "src/Api/AssuranceService.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AssuranceService.Api.dll"]

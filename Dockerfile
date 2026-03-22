# syntax=docker/dockerfile:1
# Image .NET 8 — API AssuranceService
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AssuranceService.sln", "./"]
COPY ["src/Api/AssuranceService.Api.csproj", "src/Api/"]
COPY ["src/Application/AssuranceService.Application.csproj", "src/Application/"]
COPY ["src/Domain/AssuranceService.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/AssuranceService.Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/Api/AssuranceService.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/Api/AssuranceService.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Port HTTP (aligné avec ASP.NET Core en conteneur)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Exécution non-root (utilisateur fourni par l’image aspnet)
USER app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AssuranceService.Api.dll"]

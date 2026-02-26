# Script pour generer la migration EF Core InitialCreate (AssuranceDbContext, SQL Server).
# IMPORTANT : Arretez l'application AssuranceService.Api (et fermez Visual Studio si besoin)
# avant d'executer ce script, sinon la build echouera (fichiers verrouilles).

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$infra = Join-Path $root "src\Infrastructure\AssuranceService.Infrastructure.csproj"
$api = Join-Path $root "src\Api\AssuranceService.Api.csproj"

if (-not (Test-Path $infra)) { throw "Projet Infrastructure introuvable: $infra" }
if (-not (Test-Path $api)) { throw "Projet Api introuvable: $api" }

Write-Host "[Migration] Generation de la migration InitialCreate (contexte AssuranceDbContext)..." -ForegroundColor Cyan
& dotnet ef migrations add InitialCreate `
    --project $infra `
    --startup-project $api `
    --context AssuranceDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "[Migration] Migration creee avec succes." -ForegroundColor Green
} else {
    Write-Host "[Migration] Echec. Verifiez que l'API n'est pas en cours d'execution." -ForegroundColor Red
    exit $LASTEXITCODE
}

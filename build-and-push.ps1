# Build et push de l'image AssuranceService vers Docker Hub
# Usage: .\build-and-push.ps1 [version]
# Exemple: .\build-and-push.ps1 v1.0.0

param(
    [string]$Version = "v1.0.0"
)

$ImageName = "yndinga05/assuranceservice"
$Tag = "${ImageName}:$Version"

Set-Location $PSScriptRoot

Write-Host "Build de l'image $Tag ..." -ForegroundColor Cyan
docker build -f src/Api/Dockerfile -t $Tag -t "${ImageName}:latest" .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build echoue." -ForegroundColor Red
    exit 1
}

Write-Host "Build reussi. Push vers Docker Hub..." -ForegroundColor Cyan
docker push $Tag
docker push "${ImageName}:latest"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Push echoue. Etes-vous connecte ? (docker login)" -ForegroundColor Red
    exit 1
}

Write-Host "Termine : $Tag et $ImageName`:latest pousses sur Docker Hub." -ForegroundColor Green

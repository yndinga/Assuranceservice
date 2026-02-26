# Script de déploiement AssuranceService avec PostgreSQL
# Usage: .\deploy.ps1 [-Mode standalone|shared] [-Build] [-Deploy]

param(
    [Parameter()]
    [ValidateSet('standalone', 'shared')]
    [string]$Mode = 'standalone',
    
    [Parameter()]
    [switch]$Build,
    
    [Parameter()]
    [switch]$Deploy,
    
    [Parameter()]
    [switch]$All
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Déploiement AssuranceService" -ForegroundColor Cyan
Write-Host "  Mode: $Mode" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Fonction pour vérifier Docker
function Test-Docker {
    try {
        docker version | Out-Null
        Write-Host "[✓] Docker est disponible" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "[✗] Docker n'est pas disponible ou n'est pas démarré" -ForegroundColor Red
        return $false
    }
}

# Fonction pour restaurer les packages NuGet
function Restore-Packages {
    Write-Host "`n[1/5] Restauration des packages NuGet..." -ForegroundColor Yellow
    try {
        dotnet restore
        Write-Host "[✓] Packages restaurés avec succès" -ForegroundColor Green
    }
    catch {
        Write-Host "[✗] Erreur lors de la restauration des packages" -ForegroundColor Red
        exit 1
    }
}

# Fonction pour builder l'image Docker
function Build-DockerImage {
    Write-Host "`n[2/5] Construction de l'image Docker..." -ForegroundColor Yellow
    try {
        docker build -t assuranceservice:latest -f src/Api/Dockerfile .
        Write-Host "[✓] Image construite avec succès" -ForegroundColor Green
    }
    catch {
        Write-Host "[✗] Erreur lors de la construction de l'image" -ForegroundColor Red
        exit 1
    }
}

# Fonction pour créer le réseau (mode shared)
function Create-Network {
    Write-Host "`n[3/5] Création du réseau microservices-network..." -ForegroundColor Yellow
    
    $networkExists = docker network ls --filter name=microservices-network --format "{{.Name}}" | Select-String -Pattern "microservices-network"
    
    if ($networkExists) {
        Write-Host "[✓] Le réseau existe déjà" -ForegroundColor Green
    }
    else {
        try {
            docker network create microservices-network
            Write-Host "[✓] Réseau créé avec succès" -ForegroundColor Green
        }
        catch {
            Write-Host "[✗] Erreur lors de la création du réseau" -ForegroundColor Red
            exit 1
        }
    }
}

# Fonction pour déployer l'infrastructure (mode shared)
function Deploy-Infrastructure {
    Write-Host "`n[4/5] Déploiement de l'infrastructure partagée..." -ForegroundColor Yellow
    
    $infraPath = "..\docker-compose.infrastructure.yml"
    
    if (Test-Path $infraPath) {
        try {
            docker-compose -f $infraPath up -d
            Write-Host "[✓] Infrastructure déployée avec succès" -ForegroundColor Green
            Write-Host "    - PostgreSQL: localhost:5432" -ForegroundColor Cyan
            Write-Host "    - RabbitMQ: localhost:5672 (Management: localhost:15672)" -ForegroundColor Cyan
            Write-Host "    - pgAdmin: http://localhost:5050" -ForegroundColor Cyan
            
            Write-Host "`n[⏳] Attente du démarrage de PostgreSQL..." -ForegroundColor Yellow
            Start-Sleep -Seconds 10
            
            # Créer la base de données
            Write-Host "`n[+] Création de la base de données GUOT_ASSURANCE..." -ForegroundColor Yellow
            docker exec postgres_shared psql -U postgres -c "CREATE DATABASE \"GUOT_ASSURANCE\";" 2>$null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "[✓] Base de données créée" -ForegroundColor Green
            }
            else {
                Write-Host "[!] La base de données existe déjà ou erreur" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "[✗] Erreur lors du déploiement de l'infrastructure" -ForegroundColor Red
            exit 1
        }
    }
    else {
        Write-Host "[!] Fichier docker-compose.infrastructure.yml introuvable" -ForegroundColor Yellow
        Write-Host "    Déploiement en mode standalone avec infrastructure locale" -ForegroundColor Yellow
    }
}

# Fonction pour déployer le service
function Deploy-Service {
    Write-Host "`n[5/5] Déploiement du service AssuranceService..." -ForegroundColor Yellow
    
    try {
        if ($Mode -eq 'shared') {
            docker-compose -f docker-compose.shared.yml up -d
        }
        else {
            docker-compose -f docker-compose.yml up -d
        }
        
        Write-Host "[✓] Service déployé avec succès" -ForegroundColor Green
        Write-Host "    - AssuranceService: http://localhost:8087" -ForegroundColor Cyan
        Write-Host "    - Swagger UI: http://localhost:8087/swagger" -ForegroundColor Cyan
    }
    catch {
        Write-Host "[✗] Erreur lors du déploiement du service" -ForegroundColor Red
        exit 1
    }
}

# Fonction pour afficher l'état des containers
function Show-Status {
    Write-Host "`n=====================================" -ForegroundColor Cyan
    Write-Host "  État des Containers" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    docker ps --filter "name=assuranceservice" --filter "name=postgres" --filter "name=rabbitmq"
}

# Fonction pour afficher les logs
function Show-Logs {
    Write-Host "`n=====================================" -ForegroundColor Cyan
    Write-Host "  Logs AssuranceService (Ctrl+C pour quitter)" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    docker logs -f assuranceservice
}

# Main
if (-not (Test-Docker)) {
    exit 1
}

if ($All) {
    $Build = $true
    $Deploy = $true
}

if ($Build -or $All) {
    Restore-Packages
    Build-DockerImage
}

if ($Deploy -or $All) {
    if ($Mode -eq 'shared') {
        Create-Network
        Deploy-Infrastructure
    }
    
    Deploy-Service
    
    Write-Host "`n[⏳] Attente du démarrage du service (15 secondes)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
    
    Show-Status
    
    Write-Host "`n=====================================" -ForegroundColor Green
    Write-Host "  ✓ Déploiement Terminé !" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
    Write-Host "`nServices disponibles:" -ForegroundColor White
    Write-Host "  • API: http://localhost:8087" -ForegroundColor Cyan
    Write-Host "  • Swagger: http://localhost:8087/swagger" -ForegroundColor Cyan
    
    if ($Mode -eq 'shared') {
        Write-Host "  • PostgreSQL: localhost:5432 (postgres/Yvann2018!)" -ForegroundColor Cyan
        Write-Host "  • RabbitMQ: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
        Write-Host "  • pgAdmin: http://localhost:5050 (admin@admin.com/admin)" -ForegroundColor Cyan
    }
    
    Write-Host "`nCommandes utiles:" -ForegroundColor White
    Write-Host "  • Voir les logs: docker logs -f assuranceservice" -ForegroundColor Gray
    Write-Host "  • Arrêter: docker-compose down" -ForegroundColor Gray
    Write-Host "  • Redémarrer: docker-compose restart" -ForegroundColor Gray
    
    $response = Read-Host "`nVoulez-vous voir les logs en temps réel ? (O/N)"
    if ($response -eq 'O' -or $response -eq 'o') {
        Show-Logs
    }
}
else {
    Write-Host "`nUtilisation:" -ForegroundColor Yellow
    Write-Host "  .\deploy.ps1 -All                    # Build et déploiement complet" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Build                  # Builder l'image uniquement" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Deploy                 # Déployer uniquement" -ForegroundColor White
    Write-Host "  .\deploy.ps1 -Mode shared -All       # Déploiement avec infrastructure partagée" -ForegroundColor White
}









# 🐘 AssuranceService - PostgreSQL Edition

Service de gestion des assurances pour le système GUOT, utilisant **PostgreSQL** comme base de données.

## 📋 Technologies

- **.NET 8.0** - Framework principal
- **PostgreSQL 16** - Base de données relationnelle
- **Entity Framework Core 8.0** - ORM avec Npgsql
- **RabbitMQ** - Messaging et SAGA orchestration
- **MassTransit** - Framework de messaging
- **Docker** - Containerisation

## 🚀 Démarrage Rapide

### Option 1 : Avec le Script PowerShell (Recommandé)

```powershell
# Déploiement complet (standalone)
.\deploy.ps1 -All

# Ou avec infrastructure partagée
.\deploy.ps1 -Mode shared -All
```

### Option 2 : Commandes Manuelles

#### Standalone (tout-en-un)

```bash
# 1. Restaurer les packages
dotnet restore

# 2. Builder l'image Docker
docker build -t assuranceservice:latest -f src/Api/Dockerfile .

# 3. Démarrer les services
docker-compose up -d

# 4. Vérifier les logs
docker logs -f assuranceservice
```

#### Infrastructure Partagée

```bash
# 1. Créer le réseau
docker network create microservices-network

# 2. Démarrer l'infrastructure
docker-compose -f ../docker-compose.infrastructure.yml up -d

# 3. Créer la base de données
docker exec postgres_shared psql -U postgres -c "CREATE DATABASE \"GUOT_ASSURANCE\";"

# 4. Builder et démarrer le service
docker build -t assuranceservice:latest -f src/Api/Dockerfile .
docker-compose -f docker-compose.shared.yml up -d
```

## 🔧 Configuration

### Variables d'Environnement

Fichier `docker.env` :

```env
# PostgreSQL
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_DB=GUOT_ASSURANCE
POSTGRES_USER=postgres
POSTGRES_PASSWORD=Yvann2018!

# RabbitMQ
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672

# API
API_PORT=8087
```

### Chaîne de Connexion PostgreSQL

```
Host=postgres;Port=5432;Database=GUOT_ASSURANCE;Username=postgres;Password=Yvann2018!
```

## 📦 Structure du Projet

```
AssuranceService/
├── src/
│   ├── Api/                    # API REST
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── Application/            # Logique métier
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Consumers/
│   │   └── Sagas/
│   ├── Domain/                 # Entités et modèles
│   │   ├── Models/
│   │   ├── Entities/
│   │   └── Events/
│   └── Infrastructure/         # Data Access
│       ├── Data/
│       │   └── AppDbContext.cs
│       ├── Repositories/
│       └── DependencyInjection.cs
├── docker-compose.yml          # Déploiement standalone
├── docker-compose.shared.yml   # Déploiement partagé
├── docker.env                  # Variables d'environnement
└── deploy.ps1                  # Script de déploiement
```

## 🗄️ Gestion de la Base de Données

### Connexion à PostgreSQL

```bash
# Via Docker
docker exec -it postgres_shared psql -U postgres -d GUOT_ASSURANCE

# Via pgAdmin
# URL: http://localhost:5050
# Email: admin@admin.com
# Password: admin
```

### Commandes Utiles

```sql
-- Lister les bases de données
\l

-- Se connecter à une base
\c GUOT_ASSURANCE

-- Lister les tables
\dt

-- Décrire une table
\d "Assurances"

-- Requête simple
SELECT * FROM "Assurances" LIMIT 10;
```

### Migrations

```bash
# Créer une migration
cd src/Infrastructure
dotnet ef migrations add NomDeLaMigration -s ../Api

# Appliquer les migrations
dotnet ef database update -s ../Api

# Supprimer la dernière migration
dotnet ef migrations remove -s ../Api
```

### Backup et Restore

```bash
# Backup
docker exec postgres_shared pg_dump -U postgres GUOT_ASSURANCE > backup_$(date +%Y%m%d).sql

# Restore
cat backup_20251103.sql | docker exec -i postgres_shared psql -U postgres -d GUOT_ASSURANCE
```

## 🐳 Docker

### Commandes Docker Utiles

```bash
# Voir les containers en cours
docker ps

# Voir les logs
docker logs -f assuranceservice
docker logs -f postgres_shared
docker logs -f rabbitmq_shared

# Arrêter les services
docker-compose down

# Arrêter et supprimer les volumes
docker-compose down -v

# Reconstruire sans cache
docker-compose build --no-cache

# Redémarrer un service
docker-compose restart assuranceservice
```

### Accès aux Containers

```bash
# Shell dans le container du service
docker exec -it assuranceservice /bin/bash

# Shell dans PostgreSQL
docker exec -it postgres_shared psql -U postgres

# Shell dans RabbitMQ
docker exec -it rabbitmq_shared /bin/bash
```

## 🌐 Endpoints API

Une fois démarré, l'API est accessible sur :

- **API Base** : http://localhost:8087
- **Swagger UI** : http://localhost:8087/swagger
- **Health Check** : http://localhost:8087/health

### Exemples d'Endpoints

```
GET    /api/assurances              # Liste des assurances
GET    /api/assurances/{id}         # Détail d'une assurance
POST   /api/assurances              # Créer une assurance
PUT    /api/assurances/{id}         # Modifier une assurance
DELETE /api/assurances/{id}         # Supprimer une assurance

GET    /api/marchandises            # Liste des marchandises
POST   /api/marchandises            # Ajouter une marchandise

GET    /api/primes                  # Liste des primes
POST   /api/primes                  # Calculer une prime
```

## 🔍 Monitoring

### Services de Monitoring

- **PostgreSQL** : localhost:5432
- **pgAdmin** : http://localhost:5050
- **RabbitMQ Management** : http://localhost:15672

### Healthchecks

```bash
# Vérifier PostgreSQL
docker exec postgres_shared pg_isready -U postgres

# Vérifier RabbitMQ
docker exec rabbitmq_shared rabbitmq-diagnostics ping

# Vérifier l'API
curl http://localhost:8087/health
```

## 🧪 Tests

```bash
# Restaurer les packages
dotnet restore

# Builder le projet
dotnet build

# Exécuter les tests (si présents)
dotnet test

# Tester l'API avec curl
curl -X GET http://localhost:8087/api/assurances
```

## 📚 Documentation

- [Guide de Déploiement Portainer](./DEPLOIEMENT_PORTAINER_POSTGRESQL.md)
- [Infrastructure Partagée](../docker-compose.infrastructure.yml)
- [Améliorations Planifiées](./AMELIORATIONS.md)

## 🔐 Sécurité

### En Production

⚠️ **Important** : Changez les mots de passe par défaut :

```env
# PostgreSQL
POSTGRES_PASSWORD=VotreMotDePasseSecurise123!

# RabbitMQ
RABBITMQ_USER=votre_utilisateur
RABBITMQ_PASSWORD=VotreMotDePasseSecurise456!

# pgAdmin
PGADMIN_DEFAULT_EMAIL=votre.email@entreprise.com
PGADMIN_DEFAULT_PASSWORD=VotreMotDePasseSecurise789!
```

### Bonnes Pratiques

- Utilisez des secrets Docker ou des variables d'environnement chiffrées
- Limitez les ports exposés
- Utilisez SSL/TLS pour PostgreSQL en production
- Activez l'authentification pour RabbitMQ
- Configurez les CORS correctement

## 🐛 Dépannage

### Erreur : "Cannot connect to PostgreSQL"

```bash
# Vérifier que PostgreSQL est démarré
docker ps | grep postgres

# Vérifier les logs
docker logs postgres_shared

# Tester la connexion
docker exec postgres_shared psql -U postgres -c "SELECT version();"
```

### Erreur : "Database does not exist"

```bash
# Créer la base de données
docker exec postgres_shared psql -U postgres -c "CREATE DATABASE \"GUOT_ASSURANCE\";"
```

### Erreur : "Port already allocated"

```bash
# Trouver le processus utilisant le port
netstat -ano | findstr :8087

# Changer le port dans docker.env
API_PORT=8088
```

### Réinitialisation Complète

```bash
# Arrêter et supprimer tout
docker-compose down -v

# Supprimer les images
docker rmi assuranceservice:latest

# Supprimer les volumes
docker volume rm assuranceservice_postgres_data

# Redémarrer
.\deploy.ps1 -All
```

## 📊 Performance

### Optimisations PostgreSQL

Dans `docker-compose.yml`, ajoutez :

```yaml
postgres:
  environment:
    - POSTGRES_INITDB_ARGS=--encoding=UTF8 --lc-collate=C --lc-ctype=C
  command:
    - "postgres"
    - "-c"
    - "max_connections=200"
    - "-c"
    - "shared_buffers=256MB"
    - "-c"
    - "effective_cache_size=1GB"
```

### Index Recommandés

```sql
-- Index sur les champs fréquemment recherchés
CREATE INDEX idx_assurances_nopolice ON "Assurances"("NoPolice");
CREATE INDEX idx_assurances_importateur ON "Assurances"("Importateur");
CREATE INDEX idx_marchandises_assurance ON "Marchandises"("AssuranceId");
```

## 🚀 Déploiement en Production

### Checklist

- [ ] Changer tous les mots de passe par défaut
- [ ] Configurer SSL/TLS pour PostgreSQL
- [ ] Activer l'authentification RabbitMQ
- [ ] Configurer les CORS
- [ ] Activer les logs centralisés
- [ ] Configurer les backups automatiques
- [ ] Mettre en place le monitoring (Prometheus/Grafana)
- [ ] Configurer les alertes
- [ ] Tester la haute disponibilité
- [ ] Documenter les procédures de restauration

### Backup Automatique

Créer un script cron pour les backups :

```bash
# backup.sh
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
docker exec postgres_shared pg_dump -U postgres GUOT_ASSURANCE > /backups/assurance_$DATE.sql
find /backups -name "assurance_*.sql" -mtime +7 -delete
```

## 📝 Changelog

### Version 2.0.0 (2025-11-03)

- ✅ Migration de SQL Server vers PostgreSQL
- ✅ Utilisation de Npgsql.EntityFrameworkCore.PostgreSQL
- ✅ Mise à jour des chaînes de connexion
- ✅ Scripts de déploiement PowerShell
- ✅ Documentation complète
- ✅ Support infrastructure partagée

## 👥 Contributeurs

- Équipe de développement GUOT

## 📄 Licence

Propriétaire - Tous droits réservés

---

**Date de migration PostgreSQL** : 3 Novembre 2025  
**Version** : 2.0.0  
**Framework** : .NET 8.0  
**Base de données** : PostgreSQL 16









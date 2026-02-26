# 🔄 Migration SQL Server → PostgreSQL

**Date** : 3 Novembre 2025  
**Statut** : ✅ Complétée

## 📝 Résumé des Changements

### 1. Dépendances NuGet

#### ❌ Supprimé
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.8" />
```

#### ✅ Ajouté
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.8" />
```

**Fichier modifié** : `src/Infrastructure/AssuranceService.Infrastructure.csproj`

---

### 2. Configuration Entity Framework

#### Avant (SQL Server)
```csharp
services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(connectionString, sqlOptions => 
    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));
```

#### Après (PostgreSQL)
```csharp
services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString, npgsqlOptions => 
    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30))));
```

**Fichier modifié** : `src/Infrastructure/DependencyInjection.cs`

---

### 3. Chaîne de Connexion

#### Avant (SQL Server)
```
Server=localhost,1433;Database=GUOT_ASSURANCE;User Id=sa;Password=Yvann2018!;TrustServerCertificate=True;
```

#### Après (PostgreSQL)
```
Host=localhost;Port=5432;Database=GUOT_ASSURANCE;Username=postgres;Password=Yvann2018!
```

---

### 4. Docker Compose

#### docker-compose.yml

**Avant** :
```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  container_name: sqlserver_assurance
  environment:
    - ACCEPT_EULA=Y
    - SA_PASSWORD=Yvann2018!
    - MSSQL_PID=Developer
  ports:
    - "1433:1433"
```

**Après** :
```yaml
postgres:
  image: postgres:16-alpine
  container_name: postgres_assurance
  environment:
    - POSTGRES_USER=postgres
    - POSTGRES_PASSWORD=Yvann2018!
    - POSTGRES_DB=GUOT_ASSURANCE
  ports:
    - "5432:5432"
```

---

### 5. Variables d'Environnement

#### docker.env

**Avant** :
```env
SQL_SERVER_HOST=sqlserver
SQL_SERVER_PORT=1442
SQL_SERVER_DATABASE=GUOT_ASSURANCE
SQL_SERVER_USER=sa
SQL_SERVER_PASSWORD=Yvann2018!
```

**Après** :
```env
POSTGRES_HOST=postgres
POSTGRES_PORT=5432
POSTGRES_DB=GUOT_ASSURANCE
POSTGRES_USER=postgres
POSTGRES_PASSWORD=Yvann2018!
```

---

## 📁 Fichiers Modifiés

| Fichier | Action | Description |
|---------|--------|-------------|
| `src/Infrastructure/AssuranceService.Infrastructure.csproj` | ✏️ Modifié | Package NuGet PostgreSQL |
| `src/Infrastructure/DependencyInjection.cs` | ✏️ Modifié | Configuration UseNpgsql |
| `docker-compose.yml` | ✏️ Modifié | Service PostgreSQL |
| `docker-compose.shared.yml` | ✏️ Modifié | Connexion PostgreSQL partagé |
| `docker.env` | ✏️ Modifié | Variables PostgreSQL |

---

## 📁 Fichiers Créés

| Fichier | Description |
|---------|-------------|
| `DEPLOIEMENT_PORTAINER_POSTGRESQL.md` | Guide complet de déploiement Portainer |
| `README_POSTGRESQL.md` | Documentation mise à jour |
| `deploy.ps1` | Script PowerShell de déploiement automatisé |
| `../docker-compose.infrastructure.yml` | Infrastructure partagée (PostgreSQL + RabbitMQ) |
| `../init-scripts/01-create-databases.sql` | Script d'initialisation des bases |
| `MIGRATION_POSTGRESQL.md` | Ce fichier (récapitulatif) |

---

## ✅ Vérifications Effectuées

- [x] Packages NuGet restaurés avec succès
- [x] Aucune erreur de compilation
- [x] Aucune erreur de linting
- [x] Configuration de connexion PostgreSQL validée
- [x] Fichiers Docker Compose mis à jour
- [x] Documentation créée

---

## 🚀 Étapes Suivantes

### 1. Tester Localement

```bash
# Restaurer et builder
dotnet restore
dotnet build

# Déployer avec le script
.\deploy.ps1 -All
```

### 2. Vérifier les Services

```bash
# Vérifier PostgreSQL
docker exec postgres_assurance psql -U postgres -l

# Vérifier l'API
curl http://localhost:8087/health
```

### 3. Déployer dans Portainer

Suivre le guide : `DEPLOIEMENT_PORTAINER_POSTGRESQL.md`

---

## 🔧 Compatibilité

### Types de Données

| SQL Server | PostgreSQL | Notes |
|------------|-----------|-------|
| `nvarchar(n)` | `varchar(n)` | Automatique via EF Core |
| `decimal(18,2)` | `numeric(18,2)` | Automatique via EF Core |
| `datetime2` | `timestamp` | Automatique via EF Core |
| `uniqueidentifier` | `uuid` | Automatique via EF Core |
| `bit` | `boolean` | Automatique via EF Core |

### Fonctions Spécifiques

Aucune fonction SQL Server spécifique n'a été identifiée dans le code. Entity Framework Core gère automatiquement les différences.

---

## ⚠️ Points d'Attention

### 1. Sensibilité à la Casse

PostgreSQL est **sensible à la casse** pour les noms de tables/colonnes entre guillemets doubles.

```sql
-- Correct
SELECT * FROM "Assurances";

-- Incorrect (erreur)
SELECT * FROM Assurances;
```

### 2. Identifiants

Les identifiants SQL Server `[Table]` deviennent `"Table"` en PostgreSQL.  
Entity Framework Core gère cela automatiquement.

### 3. Migrations

Les migrations existantes pour SQL Server ne fonctionneront pas avec PostgreSQL.

**Solution** :
- Supprimer le dossier `Migrations` si nécessaire
- Recréer les migrations pour PostgreSQL
- Ou utiliser `Database.EnsureCreated()` (déjà en place)

```bash
cd src/Infrastructure
dotnet ef migrations add InitialPostgreSQL -s ../Api
```

### 4. Performance

PostgreSQL utilise un modèle MVCC différent de SQL Server. Considérez :
- Index appropriés
- VACUUM réguliers
- Configuration de `shared_buffers` et `effective_cache_size`

---

## 📊 Avantages de PostgreSQL

✅ **Open Source** - Pas de coûts de licence  
✅ **Performance** - Excellent pour les charges lourdes  
✅ **Extensibilité** - Support JSON, PostGIS, etc.  
✅ **Conformité SQL** - Standard SQL élevé  
✅ **Communauté** - Large communauté active  
✅ **Multi-plateforme** - Windows, Linux, macOS  
✅ **Réplication** - Support natif master-slave  

---

## 🔍 Tests de Validation

### 1. Test de Connexion

```bash
docker exec postgres_assurance psql -U postgres -d GUOT_ASSURANCE -c "SELECT version();"
```

### 2. Test des Tables

```sql
\c GUOT_ASSURANCE
\dt
```

### 3. Test de l'API

```bash
# Health check
curl http://localhost:8087/health

# Liste des assurances
curl http://localhost:8087/api/assurances
```

---

## 📞 Support

En cas de problème :

1. Vérifier les logs : `docker logs assuranceservice`
2. Consulter `DEPLOIEMENT_PORTAINER_POSTGRESQL.md`
3. Vérifier la connectivité PostgreSQL
4. Consulter la documentation PostgreSQL : https://www.postgresql.org/docs/

---

## 📚 Ressources

- [Npgsql Documentation](https://www.npgsql.org/efcore/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Docker PostgreSQL](https://hub.docker.com/_/postgres)

---

**✅ Migration complétée avec succès !**

L'application **AssuranceService** utilise maintenant **PostgreSQL 16** et est prête pour le déploiement dans Portainer.









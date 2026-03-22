# 🚀 Démarrage Rapide - Portainer

Guide ultra-rapide pour déployer **AssuranceService** dans Portainer avec PostgreSQL.

---

## 🐳 Build image → GitHub (GHCR) → Portainer

L’image est construite et poussée automatiquement sur **GitHub Container Registry** par le workflow `.github/workflows/docker-publish-ghcr.yml`.

### 1. Pousser le code sur GitHub

```powershell
git add -A
git commit -m "Votre message"
git push origin master
```

Dès que le push est fait sur `master` (ou `main`), GitHub Actions :
- build l’image Docker à partir de `src/Api/Dockerfile`
- pousse l’image vers `ghcr.io/<votre-compte>/assuranceservice:latest` (et tag `master`)

### 2. (Optionnel) Créer une version pour Portainer

Pour une version figée (ex. v1.1.0) :

```powershell
git tag v1.1.0
git push origin v1.1.0
```

L’image sera aussi disponible en `ghcr.io/<votre-compte>/assuranceservice:v1.1.0`.

### 3. Déployer dans Portainer

- **Stacks** → **Add stack** (ou **Git repository** avec `portainer-stack.yml`).
- La stack utilise par défaut : `ghcr.io/yndinga/assuranceservice:${IMAGE_TAG:-v1.1.0}`.
- Si votre compte GitHub est `yndinga`, l’image est bien `ghcr.io/yndinga/assuranceservice`. Sinon, adaptez `portainer-stack.yml` avec votre image GHCR.
- Variable d’environnement optionnelle : `IMAGE_TAG=latest` ou `IMAGE_TAG=v1.1.0` pour choisir le tag.

Une fois l’image sur GHCR, Portainer peut la récupérer sans erreur « manifest unknown ».

### Build local (test avant push)

Pour vérifier que l’image se construit correctement avant de pousser sur GitHub :

```powershell
cd D:\dev_netcore\microservice\AssuranceService
docker build -t ghcr.io/yndinga/assuranceservice:latest -f src/Api/Dockerfile .
```

Puis pour pousser cette image vers GHCR à la main (après `docker login ghcr.io` avec un PAT) :

```powershell
docker push ghcr.io/yndinga/assuranceservice:latest
```

---

## 📦 Déployer par Git (depuis GitHub)

Pour déployer la stack directement depuis le dépôt Git dans Portainer :

1. **Portainer** → **Stacks** → **Add stack**
2. **Name** : `assuranceservice`
3. Choisir **Build method** : **Git repository**
4. Renseigner :
   - **Repository URL** : `https://github.com/yndinga/Assuranceservice.git`
   - **Compose path** : `portainer-stack.yml`
   - **Branch** : `master` (ou la branche à utiliser)
5. (Optionnel) **Environment variables** : ajouter les variables si besoin (ex. `SQL_CONNECTION_STRING`, `CONSUL_HOST`).
6. Cliquer **Deploy the stack**.

Portainer va cloner le dépôt et appliquer le fichier `portainer-stack.yml`. Les prochains déploiements ( **Update the stack** ) récupéreront les changements depuis Git.

---

## ⚡ En 5 Étapes (sans Git)

### 1️⃣ Builder l'Image (Local)

```powershell
cd D:\dev_netcore\microservice\AssuranceService
docker build -t assuranceservice:latest -f src/Api/Dockerfile .
```

### 2️⃣ Créer le Réseau dans Portainer

1. Ouvrir Portainer → **Networks**
2. Cliquer **Add network**
3. Nom : `microservices-network`
4. Driver : `bridge`
5. Cliquer **Create**

### 3️⃣ Déployer l'Infrastructure

**Portainer** → **Stacks** → **Add stack** → Nom : `infrastructure`

Coller ce code :

```yaml
version: '3.9'

services:
  postgres_shared:
    image: postgres:16-alpine
    container_name: postgres_shared
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=Yvann2018!
      - POSTGRES_DB=microservices_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - microservices-network
    restart: unless-stopped

  rabbitmq_shared:
    image: rabbitmq:3-management-alpine
    container_name: rabbitmq_shared
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - microservices-network
    restart: unless-stopped

  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: pgadmin_shared
    environment:
      - PGADMIN_DEFAULT_EMAIL=admin@admin.com
      - PGADMIN_DEFAULT_PASSWORD=admin
    ports:
      - "5050:80"
    volumes:
      - pgadmin_data:/var/lib/pgadmin
    networks:
      - microservices-network
    restart: unless-stopped

networks:
  microservices-network:
    external: true

volumes:
  postgres_data:
  rabbitmq_data:
  pgadmin_data:
```

**Deploy the stack**

### 4️⃣ Créer la Base de Données

Attendre 30 secondes, puis exécuter :

```powershell
docker exec postgres_shared psql -U postgres -c "CREATE DATABASE \"GUOT_ASSURANCE\";"
```

### 5️⃣ Déployer AssuranceService

**Portainer** → **Stacks** → **Add stack** → Nom : `assuranceservice`

Coller ce code :

```yaml
version: '3.9'

services:
  assuranceservice:
    image: assuranceservice:latest
    container_name: assuranceservice
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres_shared;Port=5432;Database=GUOT_ASSURANCE;Username=postgres;Password=Yvann2018!
      - RabbitMQ__ConnectionString=amqp://guest:guest@rabbitmq_shared:5672
    ports:
      - "8087:8080"
    networks:
      - microservices-network
    restart: unless-stopped

networks:
  microservices-network:
    external: true
```

**Deploy the stack**

---

## ✅ Vérification

### Services Disponibles

| Service | URL | Identifiants |
|---------|-----|--------------|
| **API** | http://localhost:8087 | - |
| **Swagger** | http://localhost:8087/swagger | - |
| **RabbitMQ** | http://localhost:15672 | guest / guest |
| **pgAdmin** | http://localhost:5050 | admin@admin.com / admin |

### Vérifier les Logs

Dans Portainer :
1. Aller dans **Containers**
2. Cliquer sur `assuranceservice`
3. Cliquer sur **Logs**

Vous devriez voir : `Now listening on: http://[::]:8080`

---

## 🔧 Commandes Utiles

```powershell
# Voir les containers
docker ps

# Logs en temps réel
docker logs -f assuranceservice

# Accès PostgreSQL
docker exec -it postgres_shared psql -U postgres -d GUOT_ASSURANCE

# Redémarrer un service (dans Portainer ou CLI)
docker restart assuranceservice
```

---

## 🐛 Problèmes Courants

### ❌ "Network not found"
**Solution** : Créez d'abord le réseau `microservices-network` dans Portainer

### ❌ "Cannot connect to database"
**Solution** : Vérifiez que PostgreSQL est démarré et créez la base :
```powershell
docker exec postgres_shared psql -U postgres -c "CREATE DATABASE \"GUOT_ASSURANCE\";"
```

### ❌ "Image not found"
**Solution** : Buildez l'image localement avant de déployer :
```powershell
docker build -t assuranceservice:latest -f src/Api/Dockerfile .
```

### ❌ "Port already allocated"
**Solution** : Changez le port dans la stack Portainer :
```yaml
ports:
  - "8088:8080"  # Au lieu de 8087
```

---

## 💾 Backup des données pour Portainer

Pour sauvegarder la base **MS_ASSURANCE** (SQL Server) et la restaurer côté Portainer :

1. **Sauvegarde** (sur la machine source) :
   - **Dans SSMS** : ouvrez `scripts/Backup-AssuranceDatabase.sql`, modifiez le chemin du `.bak` (ex. `N'C:\backup\MS_ASSURANCE.bak'`), exécutez (F5). Le `.bak` est créé.
   - **Dans PowerShell** :
   ```powershell
   cd scripts
   .\Backup-AssuranceDatabase.ps1 -ConnectionString "Server=...;User Id=sa;Password=...;TrustServerCertificate=True;" -BackupPathOnServer "C:\backup\MS_ASSURANCE.bak"
   ```
   Si SQL Server est dans Docker, montez un volume (ex. `-v ./backup:/var/opt/mssql/backup`) et utilisez `-BackupPathOnServer "/var/opt/mssql/backup/MS_ASSURANCE.bak"`.

2. **Copiez** le fichier `.bak` vers le serveur Portainer (ou le serveur SQL cible).

3. **Restauration** (sur le serveur cible) :
   ```powershell
   .\Restore-AssuranceDatabase.ps1 -BackupPathOnServer "C:\backup\MS_ASSURANCE.bak" -ConnectionString "Server=192.168.3.178,1434;..."
   ```

Détails : [scripts/README_Backup_Restore.md](./scripts/README_Backup_Restore.md)

---

## 📖 Documentation Complète

Pour plus de détails, consultez :
- [Backup / Restore vers Portainer](./scripts/README_Backup_Restore.md)
- [Guide Complet Portainer](./DEPLOIEMENT_PORTAINER_POSTGRESQL.md)
- [README PostgreSQL](./README_POSTGRESQL.md)
- [Migration SQL Server → PostgreSQL](./MIGRATION_POSTGRESQL.md)

---

## 🎯 Prochaines Étapes

Après le déploiement réussi :

1. ✅ Tester l'API : http://localhost:8087/swagger
2. ✅ Créer des données de test
3. ✅ Configurer les backups PostgreSQL
4. ✅ Déployer les autres microservices

---

**C'est tout ! Votre service est déployé ! 🎉**









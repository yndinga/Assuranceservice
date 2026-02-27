# 🚀 Démarrage Rapide - Portainer

Guide ultra-rapide pour déployer **AssuranceService** dans Portainer avec PostgreSQL.

---

## 🐳 Publier l'image sur GHCR (pour que Portainer voie l'image)

L'image `ghcr.io/yndinga/assuranceservice:v1.0.0` est construite et poussée par **GitHub Actions**.

- **Première fois** : poussez le workflow et créez un tag pour publier la version `v1.0.0` :
  ```powershell
  git tag v1.0.0
  git push origin v1.0.0
  ```
  Le workflow `.github/workflows/docker-publish-ghcr.yml` va builder et pousser l'image sur GHCR.

- **Ensuite** : chaque push sur `master` publie aussi le tag `latest`. Pour une nouvelle version (ex. v1.0.1) :
  ```powershell
  git tag v1.0.1
  git push origin v1.0.1
  ```

Une fois l'image sur GHCR, la stack Portainer peut la récupérer sans erreur « manifest unknown ».

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

## 📖 Documentation Complète

Pour plus de détails, consultez :
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









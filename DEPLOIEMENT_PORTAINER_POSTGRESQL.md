# 🐘 Guide de Déploiement dans Portainer avec PostgreSQL

## ✅ Modifications Effectuées

L'application **AssuranceService** a été migrée de SQL Server vers **PostgreSQL** :

- ✅ Package NuGet `Npgsql.EntityFrameworkCore.PostgreSQL` installé
- ✅ Configuration de connexion PostgreSQL dans `DependencyInjection.cs`
- ✅ Fichiers Docker Compose mis à jour
- ✅ Variables d'environnement PostgreSQL configurées

---

## 📋 Prérequis

1. **Portainer** installé et accessible (http://localhost:9000)
2. **Docker** en cours d'exécution
3. Accès à votre serveur/machine

---

## 🚀 Option 1 : Déploiement Autonome (Tout-en-Un)

Cette option déploie PostgreSQL, RabbitMQ et le service ensemble.

### Étape 1 : Builder l'image Docker

Avant de déployer dans Portainer, buildez l'image localement :

```bash
cd D:\dev_netcore\microservice\AssuranceService
docker build -t assuranceservice:latest -f src/Api/Dockerfile .
```

### Étape 2 : Créer la Stack dans Portainer

1. Connectez-vous à **Portainer**
2. Allez dans **Stacks** → **Add stack**
3. Nom : `assuranceservice-standalone`
4. Copiez le contenu du fichier `docker-compose.yml` dans l'éditeur

### Étape 3 : Ajouter les Variables d'Environnement

Dans la section "Environment variables" de Portainer :

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=Yvann2018!
POSTGRES_DB=GUOT_ASSURANCE
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
API_PORT=8087
```

### Étape 4 : Modifier pour utiliser l'image pré-buildée

Dans l'éditeur de Portainer, modifiez la section `assuranceservice` :

```yaml
assuranceservice:
  image: assuranceservice:latest  # Au lieu de la section build
  container_name: assuranceservice
  depends_on:
    postgres:
      condition: service_healthy
    rabbitmq:
      condition: service_healthy
  environment:
    - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=${POSTGRES_DB:-GUOT_ASSURANCE};Username=${POSTGRES_USER:-postgres};Password=${POSTGRES_PASSWORD:-Yvann2018!}
    - RabbitMQ__ConnectionString=amqp://${RABBITMQ_USER:-guest}:${RABBITMQ_PASSWORD:-guest}@rabbitmq:5672
  ports:
    - "${API_PORT:-8087}:8080"
  networks:
    - assurance-network
```

### Étape 5 : Déployer

Cliquez sur **Deploy the stack**

---

## 🌐 Option 2 : Infrastructure Partagée (Microservices) ⭐ Recommandé

Cette option permet de partager PostgreSQL et RabbitMQ entre plusieurs microservices.

### Étape 1 : Créer le Réseau Partagé

Dans Portainer :
1. Allez dans **Networks** → **Add network**
2. Nom : `microservices-network`
3. Driver : `bridge`
4. Cliquez sur **Create the network**

### Étape 2 : Déployer l'Infrastructure Partagée

Créez une nouvelle Stack nommée `infrastructure-shared` :

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
      - postgres_shared_data:/var/lib/postgresql/data
    networks:
      - microservices-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 10s

  rabbitmq_shared:
    image: rabbitmq:3-management
    container_name: rabbitmq_shared
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_shared_data:/var/lib/rabbitmq
    networks:
      - microservices-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

networks:
  microservices-network:
    external: true

volumes:
  postgres_shared_data:
  rabbitmq_shared_data:
```

**Déployez cette stack en premier.**

### Étape 3 : Créer la Base de Données pour AssuranceService

Une fois PostgreSQL déployé, connectez-vous et créez la base de données :

```bash
# Via Portainer Console ou Docker CLI
docker exec -it postgres_shared psql -U postgres -c "CREATE DATABASE \"GUOT_ASSURANCE\";"
```

Ou via un outil graphique comme **pgAdmin** :
- Host : `localhost`
- Port : `5432`
- Username : `postgres`
- Password : `Yvann2018!`

### Étape 4 : Builder l'image AssuranceService

```bash
cd D:\dev_netcore\microservice\AssuranceService
docker build -t assuranceservice:latest -f src/Api/Dockerfile .
```

### Étape 5 : Déployer AssuranceService

Créez une nouvelle Stack nommée `assuranceservice` dans Portainer :

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

**Déployez cette stack.**

---

## 🔄 Utiliser un Registry Docker (Optionnel mais Recommandé)

Pour faciliter les déploiements futurs, utilisez un registry Docker :

### Option A : Registry Local

```bash
# Démarrer un registry local
docker run -d -p 5000:5000 --name registry registry:2

# Builder et pousser l'image
docker build -t localhost:5000/assuranceservice:latest -f src/Api/Dockerfile .
docker push localhost:5000/assuranceservice:latest
```

Dans Portainer, utilisez : `localhost:5000/assuranceservice:latest`

### Option B : Docker Hub

```bash
# Se connecter à Docker Hub
docker login

# Taguer et pousser
docker tag assuranceservice:latest votre-username/assuranceservice:latest
docker push votre-username/assuranceservice:latest
```

Dans Portainer, utilisez : `votre-username/assuranceservice:latest`

---

## 📊 Accès aux Services

Après le déploiement réussi :

| Service | URL | Identifiants |
|---------|-----|--------------|
| **AssuranceService API** | http://localhost:8087 | - |
| **Swagger UI** | http://localhost:8087/swagger | - |
| **RabbitMQ Management** | http://localhost:15672 | guest / guest |
| **PostgreSQL** | localhost:5432 | postgres / Yvann2018! |

---

## 🔍 Vérification et Dépannage

### 1. Vérifier l'état des containers

Dans Portainer :
- Allez dans **Containers**
- Tous les containers doivent être **running** (vert)

### 2. Consulter les logs

Dans Portainer, cliquez sur un container → **Logs**

### 3. Vérifier la connexion PostgreSQL

```bash
docker exec -it postgres_shared psql -U postgres -l
```

Vous devriez voir la base `GUOT_ASSURANCE` listée.

### 4. Tester l'API

```bash
curl http://localhost:8087/health
# ou visitez http://localhost:8087/swagger
```

### 5. Erreurs courantes

#### Erreur : "Cannot connect to PostgreSQL"

**Solution** : Vérifiez que :
- Le container PostgreSQL est en cours d'exécution
- La base de données `GUOT_ASSURANCE` existe
- Les credentials sont corrects

```bash
# Créer la base si nécessaire
docker exec -it postgres_shared psql -U postgres -c "CREATE DATABASE \"GUOT_ASSURANCE\";"
```

#### Erreur : "Network not found"

**Solution** : Créez d'abord le réseau `microservices-network` dans Portainer.

#### Erreur : "Image not found"

**Solution** : Buildez l'image avant de déployer ou utilisez un registry Docker.

---

## 🔄 Mise à Jour de l'Application

Pour mettre à jour l'application :

1. **Modifier le code source**
2. **Rebuilder l'image** :
   ```bash
   docker build -t assuranceservice:latest -f src/Api/Dockerfile .
   ```
3. Dans Portainer, allez dans **Stacks** → `assuranceservice`
4. Cliquez sur **Pull and redeploy** (si registry) ou **Redeploy**

---

## 📦 Restauration des Données PostgreSQL

### Backup

```bash
docker exec postgres_shared pg_dump -U postgres GUOT_ASSURANCE > backup.sql
```

### Restore

```bash
cat backup.sql | docker exec -i postgres_shared psql -U postgres -d GUOT_ASSURANCE
```

---

## 🛠️ Configuration Avancée

### Variables d'Environnement Supplémentaires

Vous pouvez ajouter dans Portainer :

```env
# Logging
ASPNETCORE_ENVIRONMENT=Production
Logging__LogLevel__Default=Information

# PostgreSQL Performance
POSTGRES_MAX_CONNECTIONS=100

# RabbitMQ
RABBITMQ_PREFETCH_COUNT=10
```

### Monitoring avec Prometheus et Grafana

Pour monitorer PostgreSQL, ajoutez dans votre stack infrastructure :

```yaml
  postgres_exporter:
    image: prometheuscommunity/postgres-exporter
    container_name: postgres_exporter
    environment:
      - DATA_SOURCE_NAME=postgresql://postgres:Yvann2018!@postgres_shared:5432/GUOT_ASSURANCE?sslmode=disable
    ports:
      - "9187:9187"
    networks:
      - microservices-network
```

---

## ✅ Checklist de Déploiement

- [ ] Builder l'image Docker localement
- [ ] Créer le réseau `microservices-network` (si option 2)
- [ ] Déployer l'infrastructure partagée (si option 2)
- [ ] Créer la base de données `GUOT_ASSURANCE`
- [ ] Déployer la stack AssuranceService
- [ ] Vérifier les logs des containers
- [ ] Tester l'API via Swagger
- [ ] Vérifier la connexion à PostgreSQL
- [ ] Vérifier la connexion à RabbitMQ

---

## 📞 Support

En cas de problème :

1. Consultez les logs dans Portainer
2. Vérifiez les healthchecks des services
3. Testez les connexions réseau entre containers
4. Vérifiez les credentials PostgreSQL

---

**Date de migration : 3 Novembre 2025**  
**Version PostgreSQL : 16-alpine**  
**Version .NET : 8.0**









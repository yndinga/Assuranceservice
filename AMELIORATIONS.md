# ✅ Améliorations du docker-compose.yml

## 🔧 Changements appliqués

### 1. **Réseau explicite défini** 🌐
```yaml
networks:
  assurance-network:
    driver: bridge
```
- Tous les services sont sur le même réseau isolé
- Meilleur contrôle et sécurité
- Communication directe par nom de conteneur

### 2. **Variables d'environnement avec valeurs par défaut** 🔒
```yaml
${SQL_SERVER_PASSWORD:-yvann}
```
- Les credentials ne sont plus en clair
- Utilisez le fichier `docker.env` pour les surcharger
- Valeurs par défaut si le fichier n'existe pas

### 3. **Noms de conteneurs explicites**
```yaml
container_name: assuranceservice
container_name: rabbitmq
```
- Plus facile à identifier
- Meilleur pour les logs et le debugging

### 4. **Ports configurables**
```yaml
ports:
  - "${API_PORT:-8087}:8080"
```
- Changez le port dans `docker.env` sans modifier le docker-compose.yml

## 📝 Utilisation du fichier docker.env

Créez un fichier `docker.env` (ou copiez `docker.env.example`) :

```env
# SQL Server
SQL_SERVER_HOST=host.docker.internal
SQL_SERVER_PASSWORD=VotreMotDePasse

# RabbitMQ
RABBITMQ_PASSWORD=VotreAutreMotDePasse
```

Puis chargez-le :
```bash
docker-compose --env-file docker.env up -d
```

## 🐧 Compatibilité Linux

Sur Linux, `host.docker.internal` ne fonctionne pas. Deux solutions :

**Option 1 : Utiliser l'IP de la passerelle Docker**
```bash
SQL_SERVER_HOST=172.17.0.1
```

**Option 2 : Utiliser un conteneur SQL Server (recommandé)**
```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    - SA_PASSWORD=${SQL_SERVER_PASSWORD}
    - ACCEPT_EULA=Y
  networks:
    - assurance-network
```

## 📊 Architecture réseau

```
┌─────────────────────────────────────────┐
│   assurance-network (bridge)            │
│                                         │
│   ┌──────────────┐   ┌──────────────┐  │
│   │  rabbitmq    │   │ assurance    │  │
│   │  :5672       │←──│ service      │  │
│   └──────────────┘   └──────────────┘  │
│                           │             │
└───────────────────────────┼─────────────┘
                            │
                            ↓
                    host.docker.internal
                            │
                            ↓
                    ┌──────────────┐
                    │ SQL Server   │
                    │ (Windows)    │
                    │ :1433        │
                    └──────────────┘
```

## ✅ Avantages

✓ **Sécurité** : Credentials dans fichier séparé  
✓ **Flexibilité** : Variables d'environnement  
✓ **Isolation** : Réseau dédié  
✓ **Production-ready** : Facile à déployer sur K8s  
✓ **Maintenabilité** : Configuration centralisée  

## 🚀 Prochaines étapes

Pour déployer sur Kubernetes, convertissez avec :
```bash
kompose convert
```

Ou créez un Helm chart basé sur cette structure.










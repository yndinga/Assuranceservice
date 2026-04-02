# Déploiement Portainer (Stack)

## 1) Image Docker

Le dépôt contient déjà un `Dockerfile` à la racine (recommandé pour builder l’API).

Build local :

```bash
docker build -t assurance-service:local -f Dockerfile .
```

Build + tag registre :

```bash
docker build -t REGISTRY/assurance-service:1.1.0 -f Dockerfile .
docker push REGISTRY/assurance-service:1.1.0
```

Pour que GitHub Actions publie aussi le tag **`1.1.0`** sur GHCR, crée et pousse un tag Git **`v1.1.0`** (les workflows `docker-publish*.yml` génèrent les tags semver à partir de `v*`).

## 2) Stack Portainer

Dans Portainer → **Stacks** → **Add stack** :

- **Web editor** : coller `deploy/portainer/stack.yml`
- **Environment variables** : définir au minimum :
  - `ASSURANCE_IMAGE` (défaut : `ghcr.io/yndinga/assuranceservice:1.1.0`, ou autre registre/tag)
  - `ASSURANCE_HTTP_PORT` (ex: `8087`)
  - `ASSURANCE_CONNECTION_STRING` (SQL Server)
  - `MINIO_ENDPOINT`, `MINIO_ACCESS_KEY`, `MINIO_SECRET_KEY`

Optionnel :
- `RABBITMQ_ENABLED=true` + `RABBITMQ_CONNECTION_STRING`
- `CONSUL_HOST`, `CONSUL_SERVICE_ADDRESS` si service discovery Consul actif

## 3) Vérification

- **API** : `GET /` et `GET /health`
- **Swagger** : `/swagger`


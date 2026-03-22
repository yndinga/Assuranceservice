# Docker — build, GitHub Container Registry (GHCR), Portainer

## Prérequis

- Docker installé
- Compte GitHub avec un **Personal Access Token (classic)** ayant au minimum `write:packages` et `read:packages`

## Build local

À la racine du dépôt (`AssuranceService`) :

```bash
docker build -t assurance-service:local .
docker run --rm -p 8087:8080 -e ASPNETCORE_ENVIRONMENT=Development assurance-service:local
```

Swagger : `http://localhost:8087/swagger`  
Health : `http://localhost:8087/health`

## Pousser l’image sur GitHub (GHCR)

Remplacez `OWNER` par votre utilisateur ou organisation GitHub, et `assurance-service` par le nom d’image souhaité.

```bash
docker build -t ghcr.io/OWNER/assurance-service:latest .

echo VOTRE_GITHUB_TOKEN | docker login ghcr.io -u VOTRE_USER_GITHUB --password-stdin

docker push ghcr.io/OWNER/assurance-service:latest
```

Optionnel — version taggée :

```bash
docker tag ghcr.io/OWNER/assurance-service:latest ghcr.io/OWNER/assurance-service:1.0.3
docker push ghcr.io/OWNER/assurance-service:1.0.3
```

### Visibilité du package

Sur GitHub : **Packages** → package `assurance-service` → **Package settings** → rendre **public** ou donner accès aux machines qui pull (sinon `docker pull` échoue avec 403).

## Déployer avec Portainer

1. **Images** → **Pull image** : `ghcr.io/OWNER/assurance-service:latest`  
   - Si le registry est privé : **Registries** → ajouter **GitHub Container Registry** (username GitHub + token `read:packages`).

2. **Containers** → **Add container**  
   - Image : `ghcr.io/OWNER/assurance-service:latest`  
   - **Publish a new network port** : `8080` → port hôte souhaité (ex. `8087`)  
   - **Env** (exemples) :
     - `ASPNETCORE_ENVIRONMENT` = `Production`
     - `ConnectionStrings__AssuranceConnection` = votre chaîne SQL (serveur **joignable depuis le nœud Docker**, pas `localhost` si SQL est sur une autre machine)
     - `ApplyMigrationsAtStartup` = `false` si vous appliquez les migrations autrement en prod

3. Ou **Stacks** → **Web editor** avec un `docker-compose` pointant sur la même image et les mêmes variables.

## Connexion SQL depuis le conteneur

- SQL sur la **même machine** que Docker Desktop (Windows/Mac) : souvent `Server=host.docker.internal,1433;...`
- SQL sur un **serveur réseau** : IP ou hostname réel + port TCP ouvert
- Ne pas utiliser `localhost` dans la chaîne côté conteneur : `localhost` = le conteneur lui-même

## CI GitHub Actions (optionnel)

Ajoutez un workflow `.github/workflows/docker-publish.yml` qui build et push sur `ghcr.io` à chaque tag ou push sur `main` (secret `GITHUB_TOKEN` suffit pour GHCR dans le même repo).
